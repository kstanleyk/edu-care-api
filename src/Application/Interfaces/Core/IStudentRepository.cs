using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IStudentRepository : IRepository<Student, Guid>
{
    Task<Student?> GetByIdWithDetailsAsync(Guid id);
    Task<Student?> GetByStudentIdAsync(string studentId);
    Task<List<Student>> GetByClassIdAsync(Guid classId);
    Task<Student?> GetByStudentIdWithDetailsAsync(string studentId);
    Task<Student?> GetByIdWithCurrentEnrollmentAsync(Guid studentId);
    Task<bool> ExistsAsync(Guid studentId);
}