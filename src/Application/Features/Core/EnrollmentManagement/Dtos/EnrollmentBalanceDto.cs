using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

public record EnrollmentBalanceDto
{
    public Guid EnrollmentId { get; init; }
    public Guid AcademicYearId { get; init; }
    public string AcademicYearName { get; init; } = null!;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = null!;
    public string ClassCode { get; init; } = null!;
    public Money TotalFees { get; init; } = null!;
    public Money TotalPaid { get; init; } = null!;
    public Money ScholarshipDiscount { get; init; } = null!;
    public Money Balance { get; init; } = null!;
    public DateTime EnrollmentDate { get; init; }
    public bool IsActive { get; init; }
}