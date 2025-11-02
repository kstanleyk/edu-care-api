using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Commands;

public record UpdatePaymentCommand(
    Guid PaymentId,
    Money Amount,
    DateTime PaymentDate,
    string PaymentMethod,
    string? Notes) : IRequest;