using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Commands;

public record ProcessPaymentCommand(
    Guid EnrollmentId,
    Guid BursaryId,
    Money Amount,
    DateTime PaymentDate,
    string PaymentMethod,
    string ReferenceNumber,
    string? Notes = null) : IRequest<Guid>;