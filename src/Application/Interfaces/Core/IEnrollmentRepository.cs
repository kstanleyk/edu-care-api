using EduCare.Domain.Entity.Core;

namespace EduCare.Application.Interfaces.Core;

public interface IEnrollmentRepository : IRepository<Enrollment, Guid>
{
    Task<Enrollment?> GetByIdWithDetailsAsync(Guid enrollmentId);
    Task<List<Enrollment>> GetStudentEnrollmentsWithDetailsAsync(Guid studentId);
    Task<Enrollment?> GetActiveEnrollmentAsync(Guid studentId, Guid academicYearId);
    Task<List<Enrollment>> GetActiveEnrollmentsByClassIdAsync(Guid classId);
    Task<List<Enrollment>> GetEnrollmentsByAcademicYearIdAsync(Guid academicYearId);
    Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId);
}