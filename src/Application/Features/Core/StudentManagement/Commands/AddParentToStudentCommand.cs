using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.StudentManagement.Commands;

public record AddParentToStudentCommand(Guid StudentId, PersonName ParentName, string Email, string Phone, string Relationship, Address? Address, bool IsPrimaryContact = false) : IRequest<Guid>;