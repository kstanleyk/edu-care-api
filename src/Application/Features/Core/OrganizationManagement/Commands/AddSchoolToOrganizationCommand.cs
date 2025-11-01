using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Commands;

public record AddSchoolToOrganizationCommand(Guid OrganizationId, Guid SchoolId) : IRequest;