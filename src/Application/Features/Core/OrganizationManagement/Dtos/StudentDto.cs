using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record StudentDto(Guid Id, string StudentId, PersonName Name, DateOnly DateOfBirth, string? Gender, DateTime CreatedOn);