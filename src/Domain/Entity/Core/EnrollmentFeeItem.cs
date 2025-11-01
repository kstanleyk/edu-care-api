using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class EnrollmentFeeItem : Entity<Guid>
{
    public Guid EnrollmentId { get; private set; }
    public Guid FeeItemId { get; private set; }
    public Money Amount { get; private set; } = null!;

    public Enrollment Enrollment { get; private set; } = null!;
    public FeeItem FeeItem { get; private set; } = null!;

    protected EnrollmentFeeItem() { }

    /// <summary>
    /// Creates a record of an optional fee item selected by a student
    /// </summary>
    /// <param name="feeItemId">Fee item ID</param>
    /// <param name="amount">Amount of the fee item</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static EnrollmentFeeItem Create(Guid feeItemId, Money amount, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));

        return new EnrollmentFeeItem
        {
            Id = Guid.NewGuid(),
            FeeItemId = feeItemId,
            Amount = amount,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }
}