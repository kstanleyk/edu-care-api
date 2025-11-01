using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Commands;

public record CreateStudentCommand(string StudentId, PersonName Name, DateOnly DateOfBirth, string? Gender) : IRequest<Guid>;