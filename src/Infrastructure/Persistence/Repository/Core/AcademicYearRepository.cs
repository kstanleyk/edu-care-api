
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

    public async Task<AcademicYear?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .FirstOrDefaultAsync(ay => ay.Id == id);
    }

    public async Task<List<AcademicYear>> GetBySchoolIdAsync(Guid schoolId)
    {
        return await DbSet
            .Where(ay => ay.SchoolId == schoolId)
            .OrderByDescending(ay => ay.StartDate)
            .ToListAsync();
    }
}