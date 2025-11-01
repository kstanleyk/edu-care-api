using EduCare.Application.Features.Core.EnrollmentManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.PaymentManagement.Queries;

public record GetBursaryCollectionSummaryQuery(Guid BursaryId, DateOnly fromDate, DateOnly toDate)
    : IRequest<CollectionSummaryDto>;