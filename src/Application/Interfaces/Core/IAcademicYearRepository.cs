using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IAcademicYearRepository : IRepository<AcademicYear, Guid>
{
    Task<AcademicYear?> GetCurrentBySchoolIdAsync(Guid schoolId);
    Task<AcademicYear?> GetByIdAsync(Guid id);
    Task<List<AcademicYear>> GetBySchoolIdAsync(Guid schoolId);
}