using EduCare.Application.Features.Core.FeeManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Queries;

public record GetEnrollmentPaymentsQuery(Guid EnrollmentId) : IRequest<List<PaymentDto>>;