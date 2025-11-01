using MediatR;
using EduCare.Application.Features.Core.EnrollmentManagement.Dtos;

namespace EduCare.Application.Features.Core.Reporting.Queries;

public record GetClassFeeSummaryReportQuery(Guid AcademicYearId) : IRequest<ClassFeeSummaryReportDto>;