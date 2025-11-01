using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.Dtos;

public record BalanceDto
{
    public Money TotalFees { get; init; } = null!;
    public Money TotalPaid { get; init; } = null!;
    public Money ScholarshipDiscount { get; init; } = null!;
    public Money Balance { get; init; } = null!;

    // Parameterless constructor for AutoMapper
    public BalanceDto() { }

    public BalanceDto(Money totalFees, Money totalPaid, Money scholarshipDiscount, Money balance)
    {
        TotalFees = totalFees;
        TotalPaid = totalPaid;
        ScholarshipDiscount = scholarshipDiscount;
        Balance = balance;
    }
}