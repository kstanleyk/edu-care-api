using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

public record ClassFeeSummaryReportDto
{
    public Guid AcademicYearId { get; init; }
    public string AcademicYearName { get; init; } = null!;
    public Guid SchoolId { get; init; }
    public string SchoolName { get; init; } = null!;
    public DateOnly ReportDate { get; init; }
    public List<ClassFeeSummaryDto> ClassSummaries { get; init; } = new();
    public Money TotalExpectedFees { get; init; } = null!;
    public Money TotalCollected { get; init; } = null!;
    public Money TotalBalance { get; init; } = null!;
    public decimal OverallCollectionRate { get; init; }
}