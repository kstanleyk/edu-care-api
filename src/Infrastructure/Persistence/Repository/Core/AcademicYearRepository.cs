using EduCare.Application.Features.Core.AcademicYearManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class AcademicYearRepository(IDatabaseFactory databaseFactory)
    : DataRepository<AcademicYear, Guid>(databaseFactory), IAcademicYearRepository
{
    public async Task<AcademicYear?> GetCurrentBySchoolIdAsync(Guid schoolId)
    {
        return await DbSet
            .Where(ay => ay.SchoolId == schoolId)
            .Where(ay => ay.IsCurrent)
            .OrderByDescending(ay => ay.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<AcademicYear?> GetByCodeAndSchoolIdAsync(string code, Guid schoolId)
    {
        return await DbSet
            .FirstOrDefaultAsync(ay => ay.Code == code && ay.SchoolId == schoolId);
    }

    public async Task ClearCurrentAcademicYearFlagAsync(Guid schoolId, Guid excludeAcademicYearId)
    {
        await DbSet
            .Where(ay => ay.SchoolId == schoolId &&
                         ay.Id != excludeAcademicYearId &&
                         ay.IsCurrent)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(ay => ay.IsCurrent, false)
                    .SetProperty(ay => ay.ModifiedOn, DateTime.UtcNow));
    }

    public async Task<RepositoryActionResult<AcademicYear>> UpdateAcademicYearAsync(UpdateAcademicYearParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the academic year
            var academicYear = await GetByIdWithClassesAsync(parameters.Id);
            if (academicYear is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.NotFound);
            }

            // Update the academic year using domain method
            academicYear.Update(parameters.Name, parameters.StartDate, parameters.EndDate, parameters.IsCurrent);

            // If this is being marked as current, clear current flag from other academic years in the same school
            if (parameters.IsCurrent)
            {
                await ClearCurrentAcademicYearFlagAsync(academicYear.SchoolId, academicYear.Id);
            }

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<AcademicYear>(academicYear, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<AcademicYear?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .FirstOrDefaultAsync(ay => ay.Id == id);
    }

    public async Task<AcademicYear?> GetByIdWithClassesAsync(Guid id)
    {
        return await DbSet
            .Include(ay => ay.Classes)
            .FirstOrDefaultAsync(ay => ay.Id == id);
    }

    public async Task<RepositoryActionResult<AcademicYear>> MarkAsCurrentAsync(Guid academicYearId)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the academic year
            var academicYear = await GetByIdAsync(academicYearId);
            if (academicYear is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.NotFound);
            }

            // Clear current flag from other academic years in the same school
            await ClearCurrentAcademicYearFlagAsync(academicYear.SchoolId, academicYearId);

            // Mark this academic year as current using domain method
            academicYear.MarkAsCurrent();

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<AcademicYear>(academicYear, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<AcademicYear>> CreateAcademicYearAsync(AcademicYear academicYear)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check for duplicate academic year code in the same school
            var existingAcademicYear = await GetByCodeAndSchoolIdAsync(academicYear.Code, academicYear.SchoolId);
            if (existingAcademicYear is not null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.Invalid);
            }

            // Add to context
            await Context.AcademicYears.AddAsync(academicYear);

            // If this is marked as current, update other academic years in the same school
            if (academicYear.IsCurrent)
            {
                await ClearCurrentAcademicYearFlagAsync(academicYear.SchoolId, academicYear.Id);
            }

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<AcademicYear>(academicYear, RepositoryActionStatus.Created);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<List<AcademicYear>> GetBySchoolIdAsync(Guid schoolId)
    {
        return await DbSet
            .Where(ay => ay.SchoolId == schoolId)
            .OrderByDescending(ay => ay.StartDate)
            .ToListAsync();
    }
}