using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class FeeStructureItem : Entity<Guid>
{
    public Guid FeeStructureId { get; private set; }
    public Guid FeeItemId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public bool IsOptional { get; private set; }
    public int DisplayOrder { get; private set; }

    public FeeStructure FeeStructure { get; private set; } = null!;
    public FeeItem FeeItem { get; private set; } = null!;

    protected FeeStructureItem() { }

    /// <summary>
    /// Creates a new fee structure item linking a fee item to a fee structure with specific amount
    /// </summary>
    /// <param name="feeItemId">Fee item ID</param>
    /// <param name="amount">Amount for this fee item</param>
    /// <param name="isOptional">Whether this fee item is optional</param>
    /// <param name="displayOrder">Display order in the fee structure</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static FeeStructureItem Create(Guid feeItemId, Money amount, bool isOptional = false,
        int displayOrder = 0, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));

        return new FeeStructureItem
        {
            Id = Guid.NewGuid(),
            FeeItemId = feeItemId,
            Amount = amount,
            IsOptional = isOptional,
            DisplayOrder = displayOrder,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void UpdateAmount(Money newAmount)
    {
        DomainGuards.AgainstNull(newAmount, nameof(newAmount));
        Amount = newAmount;
        ModifiedOn = DateTime.UtcNow;
    }

    public void UpdateOptionality(bool isOptional)
    {
        IsOptional = isOptional;
        ModifiedOn = DateTime.UtcNow;
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        ModifiedOn = DateTime.UtcNow;
    }
}