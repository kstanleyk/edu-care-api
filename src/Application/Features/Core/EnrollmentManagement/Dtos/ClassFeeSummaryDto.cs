using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

public record ClassFeeSummaryDto
{
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = null!;
    public string ClassCode { get; init; } = null!;
    public int GradeLevel { get; init; }
    public int TotalStudents { get; init; }
    public int PaidInFullCount { get; init; }
    public int PartialPaymentCount { get; init; }
    public int NoPaymentCount { get; init; }
    public Money TotalExpectedFees { get; init; } = null!;
    public Money TotalCollected { get; init; } = null!;
    public Money TotalBalance { get; init; } = null!;
    public decimal CollectionRate { get; init; }
    public Money AverageBalancePerStudent { get; init; } = null!;
}