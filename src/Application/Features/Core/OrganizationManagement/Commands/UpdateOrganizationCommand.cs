using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.OrganizationManagement.Commands;

public record UpdateOrganizationCommand(Guid Id, string Name, Address? Address) : IRequest;