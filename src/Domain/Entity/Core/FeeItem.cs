using EduCare.Domain.Abstractions;

namespace EduCare.Domain.Entity.Core;

public class FeeItem : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Category { get; private set; } = null!; // e.g., "Tuition", "Boarding", "Extras"
    public string Code { get; private set; } = null!; // Unique code for the fee item
    public bool IsActive { get; private set; }

    protected FeeItem() { }

    /// <summary>
    /// Creates a new fee item
    /// </summary>
    /// <param name="name">Fee item name</param>
    /// <param name="description">Fee item description</param>
    /// <param name="category">Category of the fee item</param>
    /// <param name="code">Unique code for the fee item</param>
    /// <param name="isActive">Whether the fee item is active</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static FeeItem Create(string name, string description, string category,
        string code, bool isActive = true, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));
        DomainGuards.AgainstNullOrWhiteSpace(category, nameof(category));
        DomainGuards.AgainstNullOrWhiteSpace(code, nameof(code));

        return new FeeItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Category = category,
            Code = code,
            IsActive = isActive,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(string name, string description, string category, bool isActive)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));
        DomainGuards.AgainstNullOrWhiteSpace(category, nameof(category));

        Name = name;
        Description = description;
        Category = category;
        IsActive = isActive;
        ModifiedOn = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedOn = DateTime.UtcNow;
    }
}