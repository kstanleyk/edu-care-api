using EduCare.Domain.ValueObjects;
using EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

namespace EduCare.Application.Features.Core.BursaryManagement.Dtos
{
    public record StudentFeeSummaryDto(
        Guid StudentId,
        string StudentName,
        string CurrentClass,
        Money TotalBalance,
        Money CurrentTermBalance,
        List<EnrollmentBalanceDto> EnrollmentBalances);
}
