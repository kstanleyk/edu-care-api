using EduCare.Application.Features.Core.Reporting.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.Reporting.Queries;

public record GetFeeCollectionReportQuery(Guid SchoolId, DateOnly fromDate, DateOnly toDate) : IRequest<FeeCollectionReportDto>;