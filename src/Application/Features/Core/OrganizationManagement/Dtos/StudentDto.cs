using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record StudentDto(
    Guid Id,
    string StudentId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    DateOnly DateOfBirth,
    string? Gender,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    List<ParentDto> Parents,
    EnrollmentSummaryDto? CurrentEnrollment,
    List<EnrollmentSummaryDto> EnrollmentHistory
);

public record ParentDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    string Email,
    string Phone,
    string Relationship,
    bool IsPrimaryContact,
    Address? Address,
    DateTime CreatedOn
);

public record EnrollmentSummaryDto(
    Guid Id,
    string ClassName,
    string ClassCode,
    string AcademicYearName,
    DateOnly EnrollmentDate,
    bool IsActive
);