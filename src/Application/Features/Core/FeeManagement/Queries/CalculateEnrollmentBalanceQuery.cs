using EduCare.Application.Features.Core.BursaryManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Queries;

public record CalculateEnrollmentBalanceQuery(Guid EnrollmentId) : IRequest<BalanceDto>;