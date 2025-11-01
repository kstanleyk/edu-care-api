using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

public record StudentDebtorDto
{
    public Guid StudentId { get; init; }
    public string StudentIdCode { get; init; } = null!;
    public string StudentName { get; init; } = null!;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = null!;
    public string ClassCode { get; init; } = null!;
    public Guid EnrollmentId { get; init; }
    public Money TotalFees { get; init; } = null!;
    public Money TotalPaid { get; init; } = null!;
    public Money Balance { get; init; } = null!;
    public int DaysOverdue { get; init; }
    public string? PrimaryParentName { get; init; }
    public string? PrimaryParentPhone { get; init; }
    public string? PrimaryParentEmail { get; init; }
}

public record DailyCollectionDto
{
    public DateOnly Date { get; init; }
    public Money Amount { get; init; } = null!;
    public int PaymentCount { get; init; }
}