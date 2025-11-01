using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.BursaryManagement.Commands;

public record UpdateBursaryCommand(Guid Id, string Name, Address? Address) : IRequest;