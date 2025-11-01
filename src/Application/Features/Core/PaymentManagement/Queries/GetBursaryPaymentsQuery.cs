using EduCare.Application.Features.Core.FeeManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Queries;

public record GetBursaryPaymentsQuery(Guid BursaryId, DateOnly fromDate, DateOnly toDate) : IRequest<List<PaymentDto>>;