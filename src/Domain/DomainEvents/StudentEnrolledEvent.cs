namespace EduCare.Domain.DomainEvents;

public class StudentEnrolledEvent(Guid studentId, Guid classId, Guid enrollmentId) : DomainEvent
{
    public Guid StudentId { get; } = studentId;
    public Guid ClassId { get; } = classId;
    public Guid EnrollmentId { get; } = enrollmentId;
}