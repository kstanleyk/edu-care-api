using EduCare.Application.Features.Core.FeeManagement.Dtos;
using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.BursaryManagement.Dtos;

public record FeeBreakdownDto(
    Guid EnrollmentId,
    Money TotalMandatoryFees,
    Money TotalOptionalFees,
    Money TotalSelectedOptionalFees,
    Money ScholarshipDiscount,
    Money TotalNetFees,
    Money TotalPaid,
    Money Balance,
    List<FeeItemDetailDto> FeeItems
);