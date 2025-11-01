using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.BursaryManagement.Dtos;

public record BalanceDto(Money TotalFees, Money TotalPaid, Money ScholarshipDiscount, Money Balance);