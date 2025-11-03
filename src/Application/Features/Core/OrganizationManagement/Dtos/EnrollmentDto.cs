using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

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