using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IEnrollmentRepository : IRepository<Enrollment, Guid>
{
    Task<Enrollment?> GetByIdWithDetailsAsync(Guid enrollmentId);
    Task<List<Enrollment>> GetStudentEnrollmentsWithDetailsAsync(Guid studentId);
}