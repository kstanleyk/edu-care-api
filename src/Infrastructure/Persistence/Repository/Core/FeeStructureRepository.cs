using EduCare.Application.Features.Core.FeeManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class FeeStructureRepository( IFeeItemRepository feeItemRepository, IClassRepository classRepository, IDatabaseFactory databaseFactory)
    : DataRepository<FeeStructure, Guid>(databaseFactory), IFeeStructureRepository
{
    public async Task<FeeStructure?> GetActiveByClassIdAsync(Guid classId, DateTime effectiveDate)
    {
        return await DbSet
            .Include(fs => fs.FeeItems)
            .ThenInclude(fi => fi.FeeItem)
            .Include(fs => fs.Class)
            .Where(fs => fs.ClassId == classId)
            .Where(fs => fs.IsActive)
            .Where(fs => fs.EffectiveFrom <= effectiveDate)
            .Where(fs => fs.EffectiveTo == null || fs.EffectiveTo >= effectiveDate)
            .OrderByDescending(fs => fs.EffectiveFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<RepositoryActionResult<FeeStructure>> AddFeeItemToStructureAsync(AddFeeItemToStructureParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the fee structure with fee items
            var feeStructure = await GetByIdWithFeeItemsAsync(parameters.FeeStructureId);
            if (feeStructure is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NotFound);
            }

            // Get the fee item
            var feeItem = await feeItemRepository.GetByIdAsync(parameters.FeeItemId);
            if (feeItem is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NotFound);
            }

            // Check if fee item is already added to this fee structure
            if (feeStructure.FeeItems.Any(fi => fi.FeeItemId == parameters.FeeItemId))
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Invalid);
            }

            // Check if fee item is active
            if (!feeItem.IsActive)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Invalid);
            }

            // Add fee item to fee structure using domain method
            feeStructure.AddFeeItem(feeItem, parameters.Amount, parameters.IsOptional, parameters.DisplayOrder);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<FeeStructure>(feeStructure, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<FeeStructure>> CreateFeeStructureAsync(CreateFeeStructureParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Validate class exists
            var classEntity = await classRepository.GetByIdAsync(parameters.ClassId);
            if (classEntity is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NotFound);
            }

            // Check for overlapping fee structures
            var hasOverlap = await HasOverlappingFeeStructureAsync(
                parameters.ClassId,
                parameters.EffectiveFrom,
                parameters.EffectiveTo);

            if (hasOverlap)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Invalid);
            }

            // Create fee structure using domain factory method
            var feeStructure = FeeStructure.Create(
                parameters.Name,
                parameters.Description,
                parameters.ClassId,
                parameters.EffectiveFrom,
                parameters.EffectiveTo);

            // Add to context
            await DbSet.AddAsync(feeStructure);
            var result = await SaveChangesAsync();

            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<FeeStructure>(feeStructure, RepositoryActionStatus.Created);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            // Handle unique constraint violations
            //if (IsUniqueConstraintViolation(ex))
            //{
            //    return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Conflict, ex);
            //}
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<bool> HasOverlappingFeeStructureAsync(Guid classId, DateTime effectiveFrom, DateTime? effectiveTo, Guid? excludeId = null)
    {
        var query = DbSet
            .Where(fs => fs.ClassId == classId)
            .Where(fs => fs.IsActive);

        if (excludeId.HasValue)
        {
            query = query.Where(fs => fs.Id != excludeId.Value);
        }

        // Two date ranges [A1, A2] and [B1, B2] overlap if: A1 <= B2 && A2 >= B1
        var overlappingStructures = await query
            .Where(fs =>
                (fs.EffectiveTo == null && effectiveFrom <= fs.EffectiveFrom) || // New range starts before existing open-ended range
                (effectiveTo == null && fs.EffectiveFrom <= effectiveFrom) || // Existing range starts before new open-ended range
                (fs.EffectiveTo != null && effectiveTo != null && fs.EffectiveFrom <= effectiveTo && fs.EffectiveTo >= effectiveFrom) || // Both have end dates
                (fs.EffectiveTo == null && effectiveTo != null && fs.EffectiveFrom <= effectiveTo) || // Existing is open-ended, new has end date
                (effectiveTo == null && fs.EffectiveTo != null && effectiveFrom <= fs.EffectiveTo) // New is open-ended, existing has end date
            )
            .AnyAsync();

        return overlappingStructures;
    }

    public async Task<RepositoryActionResult<FeeStructure>> RemoveFeeItemFromStructureAsync(RemoveFeeItemFromStructureParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the fee structure with fee items
            var feeStructure = await GetByIdWithFeeItemsAsync(parameters.FeeStructureId);
            if (feeStructure is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NotFound);
            }

            // Check if fee item exists in the fee structure
            var feeStructureItem = feeStructure.FeeItems.FirstOrDefault(fi => fi.FeeItemId == parameters.FeeItemId);
            if (feeStructureItem is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Invalid);
            }

            // Remove fee item from fee structure using domain method
            feeStructure.RemoveFeeItem(parameters.FeeItemId);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<FeeStructure>(feeStructure, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<FeeStructure>> UpdateFeeItemAmountAsync(UpdateFeeItemAmountParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the fee structure with fee items
            var feeStructure = await GetByIdWithFeeItemsAsync(parameters.FeeStructureId);
            if (feeStructure is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NotFound);
            }

            // Check if fee item exists in the fee structure
            var feeStructureItem = feeStructure.FeeItems.FirstOrDefault(fi => fi.FeeItemId == parameters.FeeItemId);
            if (feeStructureItem is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Invalid);
            }

            // Check if the amount is actually changing
            if (feeStructureItem.Amount.Equals(parameters.NewAmount))
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NothingModified);
            }

            // Update fee item amount using domain method
            feeStructure.UpdateFeeItemAmount(parameters.FeeItemId, parameters.NewAmount);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<FeeStructure>(feeStructure, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<FeeStructure>> UpdateFeeStructureAsync(UpdateFeeStructureParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the existing fee structure
            var feeStructure = await GetByIdWithFeeItemsAsync(parameters.Id);
            if (feeStructure is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NotFound);
            }

            // Check for overlapping fee structures (excluding current one)
            var hasOverlap = await HasOverlappingFeeStructureAsync(
                feeStructure.ClassId,
                parameters.EffectiveFrom,
                parameters.EffectiveTo,
                parameters.Id); // Exclude current fee structure

            if (hasOverlap)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Invalid);
            }

            // Check if there are any actual changes
            if (feeStructure.Name == parameters.Name &&
                feeStructure.Description == parameters.Description &&
                feeStructure.EffectiveFrom == parameters.EffectiveFrom &&
                feeStructure.EffectiveTo == parameters.EffectiveTo &&
                feeStructure.IsActive == parameters.IsActive)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NothingModified);
            }

            // Update fee structure using domain method
            feeStructure.Update(
                parameters.Name,
                parameters.Description,
                parameters.EffectiveFrom,
                parameters.EffectiveTo,
                parameters.IsActive);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<FeeStructure>(feeStructure, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeStructure>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<FeeStructure?> GetByIdWithFeeItemsAsync(Guid id)
    {
        return await DbSet
            .Include(fs => fs.FeeItems)
                .ThenInclude(fsi => fsi.FeeItem)
            .Include(fs => fs.Class)
            .FirstOrDefaultAsync(fs => fs.Id == id);
    }

    public override async Task<FeeStructure?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(fs => fs.FeeItems)
            .ThenInclude(fi => fi.FeeItem)
            .Include(fs => fs.Class)
            .FirstOrDefaultAsync(fs => fs.Id == id);
    }

    public async Task<List<FeeStructure>> GetByClassIdAsync(Guid classId)
    {
        return await DbSet
            .Include(fs => fs.FeeItems)
            .ThenInclude(fi => fi.FeeItem)
            .Where(fs => fs.ClassId == classId)
            .OrderByDescending(fs => fs.EffectiveFrom)
            .ToListAsync();
    }

}