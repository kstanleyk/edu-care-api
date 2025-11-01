using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Commands;

public record UpdateStudentCommand(Guid Id, PersonName Name, DateOnly DateOfBirth, string? Gender) : IRequest;