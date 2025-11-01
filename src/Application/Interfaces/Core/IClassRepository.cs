using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IClassRepository : IRepository<Class, Guid>
{
    Task<List<Class>> GetByAcademicYearIdAsync(Guid academicYearId);
}