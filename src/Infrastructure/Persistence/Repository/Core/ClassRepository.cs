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
}