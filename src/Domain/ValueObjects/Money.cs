using EduCare.Domain.Entity;

namespace EduCare.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { }

    public Money(decimal amount, string currency = "XAF")
    {
        DomainGuards.AgainstNegative(amount, nameof(amount));
        DomainGuards.AgainstNullOrWhiteSpace(currency, nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}