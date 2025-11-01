using MediatR;
using EduCare.Application.Features.Core.FeeManagement.Dtos;

namespace EduCare.Application.Features.Core.PaymentManagement.Queries;

public record GetPaymentQuery(Guid PaymentId) : IRequest<PaymentDto>;