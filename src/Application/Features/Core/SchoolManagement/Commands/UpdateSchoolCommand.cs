using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.SchoolManagement.Commands;

public record UpdateSchoolCommand(Guid Id, string Name, SchoolType Type, SchoolMode Mode, Address? Address) : IRequest;