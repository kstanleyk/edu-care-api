using EduCare.Domain.ValueObjects;
using EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

namespace EduCare.Application.Features.Core.Reporting.Dtos
{
    public record ScholarshipReportDto(
        Guid AcademicYearId,
        string AcademicYearName,
        int TotalScholarships,
        Money TotalScholarshipValue,
        List<ScholarshipSummaryDto> Scholarships
    );
}
