using EduCare.Application.Features.Core.AcademicYearManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IAcademicYearRepository : IRepository<AcademicYear, Guid>
{
    Task<AcademicYear?> GetCurrentBySchoolIdAsync(Guid schoolId);
    Task<AcademicYear?> GetByIdAsync(Guid id);
    Task<List<AcademicYear>> GetBySchoolIdAsync(Guid schoolId);
    Task<AcademicYear?> GetByCodeAndSchoolIdAsync(string code, Guid schoolId);
    Task ClearCurrentAcademicYearFlagAsync(Guid schoolId, Guid excludeAcademicYearId);
    Task<RepositoryActionResult<AcademicYear>> CreateAcademicYearAsync(CreateAcademicYearCommand command);
}