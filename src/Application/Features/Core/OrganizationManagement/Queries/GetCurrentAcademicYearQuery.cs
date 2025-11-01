using MediatR;
using EduCare.Application.Features.Core.OrganizationManagement.Dtos;

namespace EduCare.Application.Features.Core.OrganizationManagement.Queries;

public record GetCurrentAcademicYearQuery(Guid SchoolId) : IRequest<AcademicYearDto?>;