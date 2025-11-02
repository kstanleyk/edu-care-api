using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class OrganizationRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Organization, Guid>(databaseFactory), IOrganizationRepository
{
    public async Task<Organization?> GetByIdWithDetailsAsync(Guid id)
    {
        return await DbSet
            .Include(o => o.Schools)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Organization?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Organization?> GetByCodeAsync(string code)
    {
        return await DbSet
            .Include(o => o.Schools)
            .FirstOrDefaultAsync(o => o.Code == code);
    }

    public async Task<List<Organization>> GetAllAsync()
    {
        return await DbSet
            .Include(o => o.Schools)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

}