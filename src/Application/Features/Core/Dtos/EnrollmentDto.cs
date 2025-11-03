using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.Dtos;

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