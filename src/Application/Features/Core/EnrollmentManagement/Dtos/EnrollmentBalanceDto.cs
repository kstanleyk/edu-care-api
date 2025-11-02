using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

public record EnrollmentBalanceDto(
    Guid EnrollmentId,
    Guid AcademicYearId,
    string AcademicYearName,
    Guid ClassId,
    string ClassName,
    string ClassCode,
    int GradeLevel,
    Money TotalFees,
    Money TotalPaid,
    Money ScholarshipDiscount,
    Money Balance,
    DateTime EnrollmentDate,
    bool IsActive
);