using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class Scholarship : Entity<Guid>
{
    public Guid EnrollmentId { get; private set; }
    public ScholarshipType Type { get; private set; }
    public decimal Percentage { get; private set; }
    public string Description { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public Enrollment Enrollment { get; private set; } = null!;

    protected Scholarship() { }

    /// <summary>
    /// Creates a new scholarship for a student enrollment
    /// </summary>
    /// <param name="enrollmentId">Enrollment ID</param>
    /// <param name="type">Type of scholarship (Full/Partial)</param>
    /// <param name="percentage">Discount percentage</param>
    /// <param name="description">Scholarship description</param>
    /// <param name="isActive">Whether the scholarship is active</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static Scholarship Create(Guid enrollmentId, ScholarshipType type, decimal percentage,
        string description, bool isActive = true, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));

        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Percentage must be between 0 and 100");

        return new Scholarship
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollmentId,
            Type = type,
            Percentage = percentage,
            Description = description,
            IsActive = isActive,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(ScholarshipType type, decimal percentage, string description, bool isActive)
    {
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));

        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Percentage must be between 0 and 100");

        Type = type;
        Percentage = percentage;
        Description = description;
        IsActive = isActive;
        ModifiedOn = DateTime.UtcNow;
    }
}