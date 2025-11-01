using EduCare.Application.Features.Core.Reporting.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.Reporting.Queries;

public record GetStudentDebtorsReportQuery(Guid AcademicYearId, Guid? ClassId = null) : IRequest<StudentDebtorsReportDto>;