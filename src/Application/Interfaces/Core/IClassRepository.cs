using EduCare.Application.Features.Core.AcademicYearManagement.Commands;
using EduCare.Application.Helpers;
using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IClassRepository : IRepository<Class, Guid>
{
    Task<List<Class>> GetByAcademicYearIdAsync(Guid academicYearId);
    Task<Class?> GetByCodeAndAcademicYearIdAsync(string code, Guid academicYearId);
    Task<RepositoryActionResult<Class>> CreateClassAsync(Class classEntity);
}