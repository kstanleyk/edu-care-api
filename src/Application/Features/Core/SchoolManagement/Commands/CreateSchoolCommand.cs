using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.SchoolManagement.Commands;

public record CreateSchoolCommand(string Name, string Code, SchoolType Type, SchoolMode Mode, Guid OrganizationId, Address? Address) : IRequest<Guid>;