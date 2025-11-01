using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.DomainEvents;

public class PaymentAppliedEvent(Guid debtId, Money amount, Money remainingBalance) : DomainEvent
{
    public Guid DebtId { get; } = debtId;
    public Money Amount { get; } = amount;
    public Money RemainingBalance { get; } = remainingBalance;
}