using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record EnrollmentDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string StudentCode,
    Guid ClassId,
    string ClassName,
    string ClassCode,
    Guid AcademicYearId,
    string AcademicYearName,
    string AcademicYearCode,
    Guid FeeStructureId,
    string FeeStructureName,
    DateOnly EnrollmentDate,
    bool IsActive,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    Money TotalFees,
    Money TotalPaid,
    Money Balance,
    IReadOnlyCollection<ScholarshipDto> Scholarships,
    IReadOnlyCollection<PaymentDto> Payments,
    IReadOnlyCollection<EnrollmentFeeItemDto> SelectedOptionalFees
);

public record ScholarshipDto(
    Guid Id,
    ScholarshipType Type,
    decimal Percentage,
    string Description,
    bool IsActive,
    DateTime CreatedOn,
    DateTime? ModifiedOn
);

public record PaymentDto(
    Guid Id,
    Money Amount,
    DateTime PaymentDate,
    string PaymentMethod,
    string ReferenceNumber,
    string? Notes,
    Guid BursaryId,
    string BursaryName,
    DateTime CreatedOn,
    DateTime? ModifiedOn
);

public record EnrollmentFeeItemDto(
    Guid Id,
    Guid FeeItemId,
    string FeeItemName,
    string FeeItemCode,
    Money Amount,
    DateTime CreatedOn
);