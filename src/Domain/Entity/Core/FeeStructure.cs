using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class FeeStructure : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Guid ClassId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    public Class Class { get; private set; } = null!;

    private readonly List<FeeStructureItem> _feeItems = [];
    public IReadOnlyCollection<FeeStructureItem> FeeItems => _feeItems.AsReadOnly();

    protected FeeStructure() { }

    /// <summary>
    /// Creates a new fee structure for a class with multiple fee items
    /// </summary>
    /// <param name="name">Name of the fee structure</param>
    /// <param name="description">Description of the fee structure</param>
    /// <param name="classId">Class ID this fee structure applies to</param>
    /// <param name="effectiveFrom">When this fee structure becomes effective</param>
    /// <param name="effectiveTo">When this fee structure expires (optional)</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static FeeStructure Create(string name, string description, Guid classId,
        DateTime effectiveFrom, DateTime? effectiveTo = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));

        if (effectiveTo.HasValue && effectiveFrom >= effectiveTo.Value)
            throw new ArgumentException("Effective from date must be before effective to date");

        return new FeeStructure
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ClassId = classId,
            IsActive = true,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(string name, string description, DateTime effectiveFrom, DateTime? effectiveTo, bool isActive)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));

        if (effectiveTo.HasValue && effectiveFrom >= effectiveTo.Value)
            throw new ArgumentException("Effective from date must be before effective to date");

        Name = name;
        Description = description;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        IsActive = isActive;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddFeeItem(FeeItem feeItem, Money amount, bool isOptional = false, int displayOrder = 0)
    {
        DomainGuards.AgainstNull(feeItem, nameof(feeItem));
        DomainGuards.AgainstNull(amount, nameof(amount));

        // Business rule: Cannot add duplicate fee items to the same fee structure
        if (_feeItems.Any(fi => fi.FeeItemId == feeItem.Id))
            throw new InvalidOperationException("Fee item already exists in this fee structure");

        var feeStructureItem = FeeStructureItem.Create(feeItem.Id, amount, isOptional, displayOrder);
        _feeItems.Add(feeStructureItem);
    }

    public void RemoveFeeItem(Guid feeItemId)
    {
        var feeItem = _feeItems.FirstOrDefault(fi => fi.FeeItemId == feeItemId);
        if (feeItem != null)
        {
            _feeItems.Remove(feeItem);
        }
    }

    public void UpdateFeeItemAmount(Guid feeItemId, Money newAmount)
    {
        var feeItem = _feeItems.FirstOrDefault(fi => fi.FeeItemId == feeItemId);
        if (feeItem != null)
        {
            feeItem.UpdateAmount(newAmount);
        }
    }

    public Money CalculateTotalFees()
    {
        var totalAmount = _feeItems
            .Where(fi => !fi.IsOptional)
            .Sum(fi => fi.Amount.Amount);

        return new Money(totalAmount);
    }

    public Money CalculateTotalWithOptionalFees()
    {
        var totalAmount = _feeItems.Sum(fi => fi.Amount.Amount);
        return new Money(totalAmount);
    }

    public bool IsEffective(DateTime date)
    {
        return date >= EffectiveFrom &&
               (!EffectiveTo.HasValue || date <= EffectiveTo.Value);
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedOn = DateTime.UtcNow;
    }
}