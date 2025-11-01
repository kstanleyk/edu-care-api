using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.DomainEvents;

public class DebtCreatedEvent(Guid debtId, Guid enrollmentId, Money totalAmount) : DomainEvent
{
    public Guid DebtId { get; } = debtId;
    public Guid EnrollmentId { get; } = enrollmentId;
    public Money TotalAmount { get; } = totalAmount;
}