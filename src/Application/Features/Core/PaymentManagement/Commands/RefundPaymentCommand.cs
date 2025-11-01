using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Commands;

public record RefundPaymentCommand(Guid PaymentId, string Reason) : IRequest<Guid>;