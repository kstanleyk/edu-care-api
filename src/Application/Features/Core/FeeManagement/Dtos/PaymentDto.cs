using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.FeeManagement.Dtos;

public record PaymentDto(
    Guid Id,
    Guid EnrollmentId,
    Guid BursaryId,
    Money Amount,
    DateTime PaymentDate,
    string PaymentMethod,
    string ReferenceNumber,
    string? Notes,
    DateTime CreatedOn);