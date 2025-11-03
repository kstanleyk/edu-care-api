using EduCare.Application.Features.Core.BursaryManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class BursaryRepository(ISchoolRepository schoolRepository, IDatabaseFactory databaseFactory)
    : DataRepository<Bursary, Guid>(databaseFactory), IBursaryRepository
{
    public async Task<Bursary?> GetByIdWithSchoolsAsync(Guid id)
    {
        return await DbSet
            .Include(b => b.Schools)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<RepositoryActionResult<Bursary>> CreateBursaryAsync(CreateBursaryParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check for duplicate bursary code
            var existingBursary = await GetByCodeAsync(parameters.Code);
            if (existingBursary is not null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.Invalid);
            }

            // Create bursary using domain factory method
            var bursary = Bursary.Create(parameters.Name, parameters.Code, parameters.Address);

            // Add to context
            await DbSet.AddAsync(bursary);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Bursary>(bursary, RepositoryActionStatus.Created);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<Bursary?> GetByCodeAsync(string code)
    {
        return await DbSet
            .FirstOrDefaultAsync(b => b.Code == code);
    }

    public async Task<RepositoryActionResult<Bursary>> AssignSchoolAsync(AssignSchoolToBursaryParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the bursary with schools
            var bursary = await GetByIdWithSchoolsAsync(parameters.BursaryId);
            if (bursary is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.NotFound);
            }

            // Get the school
            var school = await schoolRepository.GetByIdAsync(parameters.SchoolId);
            if (school is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.NotFound);
            }

            // Check if school is already assigned to this bursary
            if (bursary.Schools.Any(s => s.Id == parameters.SchoolId))
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.Invalid);
            }

            // Assign school to bursary using domain method
            bursary.AddSchool(school);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Bursary>(bursary, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Bursary>> UpdateBursaryAsync(UpdateBursaryParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the bursary with schools
            var bursary = await GetByIdWithSchoolsAsync(parameters.Id);
            if (bursary is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.NotFound);
            }

            // Update the bursary using domain method
            bursary.Update(parameters.Name, parameters.Address);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Bursary>(bursary, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Bursary>(null, RepositoryActionStatus.Error, ex);
        }
    }
}