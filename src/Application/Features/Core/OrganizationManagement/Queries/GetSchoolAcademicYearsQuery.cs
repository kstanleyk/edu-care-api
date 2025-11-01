using EduCare.Application.Features.Core.OrganizationManagement.Dtos;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Queries;

public record GetSchoolAcademicYearsQuery(Guid SchoolId) : IRequest<List<AcademicYearDto>>;