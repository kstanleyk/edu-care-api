using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class Bursary : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public Address? Address { get; private set; }

    private readonly List<School> _schools = [];
    public IReadOnlyCollection<School> Schools => _schools.AsReadOnly();

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    protected Bursary() { }

    /// <summary>
    /// Creates a new bursary
    /// </summary>
    /// <param name="name">Bursary name</param>
    /// <param name="code">Unique bursary code</param>
    /// <param name="address">Bursary address</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static Bursary Create(string name, string code, Address? address = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(code, nameof(code));

        return new Bursary
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Address = address,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(string name, Address? address)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name;
        Address = address;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddSchool(School school)
    {
        DomainGuards.AgainstNull(school, nameof(school));
        _schools.Add(school);
    }

    public void RemoveSchool(School school)
    {
        DomainGuards.AgainstNull(school, nameof(school));
        _schools.Remove(school);
    }

    public void AddPayment(Payment payment)
    {
        DomainGuards.AgainstNull(payment, nameof(payment));
        _payments.Add(payment);
    }

    public Money CalculateTotalCollections(DateTime fromDate, DateTime toDate)
    {
        var total = _payments
            .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
            .Sum(p => p.Amount.Amount);

        return new Money(total);
    }
}