
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

    public async Task<AcademicYear?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .FirstOrDefaultAsync(ay => ay.Id == id);
    }

    public async Task<RepositoryActionResult<AcademicYear>> CreateAcademicYearAsync(CreateAcademicYearCommand command)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check for duplicate academic year code in the same school
            var existingAcademicYear = await GetByCodeAndSchoolIdAsync(command.Code, command.SchoolId);
            if (existingAcademicYear is not null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<AcademicYear>(null, RepositoryActionStatus.Invalid);
            }

            // Create academic year using domain factory method
            var academicYear = AcademicYear.Create(
                command.Name,
                command.Code,
                command.StartDate,
                command.EndDate,
                command.SchoolId,
                command.IsCurrent);

            // Add to context
            await Context.AcademicYears.AddAsync(academicYear);

            // If this is marked as current, update other academic years in the same school
            if (command.IsCurrent)
            {
                await ClearCurrentAcademicYearFlagAsync(command.SchoolId, academicYear.Id);
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