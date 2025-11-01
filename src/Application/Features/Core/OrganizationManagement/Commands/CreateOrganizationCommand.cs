using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Commands;

public record CreateOrganizationCommand(string Name, string Code, Address? Address) : IRequest<Guid>;