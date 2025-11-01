using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

public record ClassCollectionDto
{
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = null!;
    public string ClassCode { get; init; } = null!;
    public int StudentCount { get; init; }
    public Money TotalExpectedFees { get; init; } = null!;
    public Money TotalCollected { get; init; } = null!;
    public Money TotalBalance { get; init; } = null!;
    public decimal CollectionRate { get; init; } // Percentage
    public int PaymentsCount { get; init; }
}