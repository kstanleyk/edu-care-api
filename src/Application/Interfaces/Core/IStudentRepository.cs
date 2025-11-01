using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IStudentRepository : IRepository<Student, Guid>
{
    
}