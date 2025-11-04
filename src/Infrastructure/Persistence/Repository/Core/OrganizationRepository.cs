using EduCare.Application.Features.Core.OrganizationManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class OrganizationRepository(ISchoolRepository schoolRepository,  IDatabaseFactory databaseFactory)
    : DataRepository<Organization, Guid>(databaseFactory), IOrganizationRepository
{
    public async Task<Organization?> GetByIdWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(o => o.Schools)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Organization?> GetByIdWithSchoolsAsync(Guid id)
    {
        return await DbSet
            .Include(o => o.Schools)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<RepositoryActionResult<Organization>> AddSchoolToOrganizationAsync(AddSchoolToOrganizationParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the organization with schools
            var organization = await GetByIdWithSchoolsAsync(parameters.OrganizationId);
            if (organization is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.NotFound);
            }

            // Get the school
            var school = await schoolRepository.GetByIdAsync(parameters.SchoolId);
            if (school is null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.NotFound);
            }

            // Check if school is already part of the organization
            if (organization.Schools.Any(s => s.Id == parameters.SchoolId))
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.Invalid);
            }

            // Check if school already belongs to another organization
            if (school.OrganizationId != Guid.Empty && school.OrganizationId != parameters.OrganizationId)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.Invalid);
            }

            // Add school to organization using domain method
            organization.AddSchool(school);

            // Update the school's OrganizationId using the domain method
            // We'll use the ChangeOrganization method we discussed earlier
            school.ChangeOrganization(parameters.OrganizationId);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Organization>(organization, RepositoryActionStatus.Updated);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Organization>> CreateOrganizationAsync(CreateOrganizationParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check if organization with same code already exists
            var existingOrganization = await GetByCodeAsync(parameters.Code);
            if (existingOrganization is not null)
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.Conflict);
            }

            // Create organization using domain factory method
            var organization = Organization.Create(
                parameters.Name,
                parameters.Code,
                parameters.Address);

            // Add to context
            await DbSet.AddAsync(organization);
            var result = await SaveChangesAsync();

            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<Organization>(organization, RepositoryActionStatus.Created);
            }

            await tx.RollbackAsync();
            return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();

            // Handle unique constraint violations for PostgreSQL
            if (IsUniqueConstraintViolation(ex))
            {
                return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.Conflict, ex);
            }
            return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Organization>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<Organization?> GetByCodeAsync(string code)
    {
        return await DbSet
            .Include(o => o.Schools)
            .FirstOrDefaultAsync(o => o.Code == code);
    }
}