using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class ClassRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Class, Guid>(databaseFactory), IClassRepository
{
    public async Task<List<Class>> GetByAcademicYearIdAsync(Guid academicYearId)
    {
        return await DbSet
            .Where(c => c.AcademicYearId == academicYearId)
            .OrderBy(c => c.GradeLevel)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<RepositoryActionResult<Class>> CreateClassAsync(Class classEntity)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check for duplicate class code in the same academic year
            var existingClass = await GetByCodeAndAcademicYearIdAsync(classEntity.Code, classEntity.AcademicYearId);
            if (existingClass is not null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Class>(null, RepositoryActionStatus.Invalid);
            }

            //// Create class using domain factory method
            //var classEntity = Class.Create(
            //    command.Name,
            //    command.Code,
            //    command.GradeLevel,
            //    command.AcademicYearId);

            // Add to context
            await DbSet.AddAsync(classEntity);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Class>(classEntity, RepositoryActionStatus.Created);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Class>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Class>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Class>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Class>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<Class?> GetByCodeAndAcademicYearIdAsync(string code, Guid academicYearId)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Code == code && c.AcademicYearId == academicYearId);
    }
}