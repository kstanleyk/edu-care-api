using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

public record ScholarshipSummaryDto
{
    public Guid ScholarshipId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = null!;
    public string StudentIdCode { get; init; } = null!;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = null!;
    public ScholarshipType Type { get; init; }
    public decimal Percentage { get; init; }
    public string Description { get; init; } = null!;
    public Money DiscountAmount { get; init; } = null!;
    public Money TotalFees { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime CreatedOn { get; init; }
}