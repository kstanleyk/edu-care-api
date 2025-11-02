using EduCare.Application.Interfaces.Core;
using EduCare.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace EduCare.Infrastructure.Persistence.Repository.Core;

public class SchoolRepository(IDatabaseFactory databaseFactory)
    : DataRepository<School, Guid>(databaseFactory), ISchoolRepository
{
    public async Task<List<School>> GetByOrganizationIdAsync(Guid organizationId)
    {
        return await DbSet
            .Where(s => s.OrganizationId == organizationId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<School>> GetByOrganizationIdWithDetailsAsync(Guid organizationId)
    {
        return await DbSet
            .Include(s => s.AcademicYears)
            .Where(s => s.OrganizationId == organizationId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<School?> GetByCodeAsync(string code)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.Code == code);
    }
}