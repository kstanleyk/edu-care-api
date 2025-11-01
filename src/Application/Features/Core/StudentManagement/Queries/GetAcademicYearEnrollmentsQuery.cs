using MediatR;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;

namespace EduCare.Application.Features.Core.StudentManagement.Queries;

public record GetAcademicYearEnrollmentsQuery(Guid AcademicYearId) : IRequest<List<EnrollmentDto>>;