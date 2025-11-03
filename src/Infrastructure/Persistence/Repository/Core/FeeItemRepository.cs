using EduCare.Application.Features.Core.FeeManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class FeeItemRepository(IDatabaseFactory databaseFactory)
    : DataRepository<FeeItem, Guid>(databaseFactory), IFeeItemRepository
{
    public async Task<FeeItem?> GetByCodeAsync(string code)
    {
        return await DbSet
            .FirstOrDefaultAsync(fi => fi.Code == code);
    }

    public async Task<RepositoryActionResult<FeeItem>> CreateFeeItemAsync(CreateFeeItemParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check if fee item with same code already exists
            var existingFeeItem = await GetByCodeAsync(parameters.Code);
            if (existingFeeItem is not null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.Conflict);
            }

            // Create fee item using domain factory method
            var feeItem = FeeItem.Create(
                parameters.Name,
                parameters.Description,
                parameters.Category,
                parameters.Code);

            // Add to context
            await DbSet.AddAsync(feeItem);
            var result = await SaveChangesAsync();

            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<FeeItem>(feeItem, RepositoryActionStatus.Created);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            // Handle unique constraint violations for PostgreSQL
            if (IsUniqueConstraintViolation(ex))
            {
                return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.Conflict, ex);
            }
            return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<FeeItem>> UpdateFeeItemAsync(UpdateFeeItemParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the existing fee item
            var feeItem = await GetByIdAsync(parameters.Id);
            if (feeItem is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.NotFound);
            }

            // Check if there are any actual changes
            if (feeItem.Name == parameters.Name &&
                feeItem.Description == parameters.Description &&
                feeItem.Category == parameters.Category &&
                feeItem.IsActive == parameters.IsActive)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.NothingModified);
            }

            // Update fee item using domain method
            feeItem.Update(
                parameters.Name,
                parameters.Description,
                parameters.Category,
                parameters.IsActive);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<FeeItem>(feeItem, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            // Handle unique constraint violations for PostgreSQL
            if (IsUniqueConstraintViolation(ex))
            {
                return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.Conflict, ex);
            }
            return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<FeeItem>(null, RepositoryActionStatus.Error, ex);
        }
    }

    /// <summary>
    /// Checks if an exception is due to a unique constraint violation in PostgreSQL
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException postgresException &&
               postgresException.SqlState == "23505"; // PostgreSQL unique constraint violation
    }
}