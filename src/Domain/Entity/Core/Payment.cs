using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class Payment : Entity<Guid>
{
    public Guid EnrollmentId { get; private set; }
    public Guid BursaryId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTime PaymentDate { get; private set; }
    public string PaymentMethod { get; private set; } = null!;
    public string ReferenceNumber { get; private set; } = null!;
    public string? Notes { get; private set; }

    public Enrollment Enrollment { get; private set; } = null!;
    public Bursary Bursary { get; private set; } = null!;

    protected Payment() { }

    /// <summary>
    /// Creates a new payment record
    /// </summary>
    /// <param name="enrollmentId">Enrollment ID</param>
    /// <param name="bursaryId">Bursary ID where payment was made</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="paymentDate">Date of payment</param>
    /// <param name="paymentMethod">Payment method used</param>
    /// <param name="referenceNumber">Payment reference number</param>
    /// <param name="notes">Additional notes</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static Payment Create(Guid enrollmentId, Guid bursaryId, Money amount, DateTime paymentDate,
        string paymentMethod, string referenceNumber, string? notes = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));
        DomainGuards.AgainstNullOrWhiteSpace(paymentMethod, nameof(paymentMethod));
        DomainGuards.AgainstNullOrWhiteSpace(referenceNumber, nameof(referenceNumber));

        return new Payment
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollmentId,
            BursaryId = bursaryId,
            Amount = amount,
            PaymentDate = paymentDate,
            PaymentMethod = paymentMethod,
            ReferenceNumber = referenceNumber,
            Notes = notes,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(Money amount, DateTime paymentDate, string paymentMethod, string? notes)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));
        DomainGuards.AgainstNullOrWhiteSpace(paymentMethod, nameof(paymentMethod));

        Amount = amount;
        PaymentDate = paymentDate;
        PaymentMethod = paymentMethod;
        Notes = notes;
        ModifiedOn = DateTime.UtcNow;
    }
}