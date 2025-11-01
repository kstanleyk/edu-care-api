using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.BursaryManagement.Commands;

public record CreateBursaryCommand(string Name, string Code, Address? Address) : IRequest<Guid>;