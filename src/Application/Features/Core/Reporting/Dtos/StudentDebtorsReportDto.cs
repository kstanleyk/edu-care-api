using EduCare.Application.Features.Core.EnrollmentManagement.Dtos;
using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.Reporting.Dtos;

public record StudentDebtorsReportDto(
    Guid AcademicYearId,
    string AcademicYearName,
    int TotalStudents,
    int DebtorCount,
    Money TotalDebt,
    List<StudentDebtorDto> Debtors
);