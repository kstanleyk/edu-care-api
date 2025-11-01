using EduCare.Application.Features.Core.EnrollmentManagement.Dtos;
using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.Reporting.Dtos;

public record FeeCollectionReportDto(
    Guid SchoolId,
    string SchoolName,
    DateOnly FromDate,
    DateOnly ToDate,
    Money TotalCollections,
    int TotalPayments,
    List<ClassCollectionDto> ClassCollections
);