using EduCare.Application.Features.Core.Reporting.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.Reporting.Queries;

public record GetScholarshipReportQuery(Guid AcademicYearId) : IRequest<ScholarshipReportDto>;