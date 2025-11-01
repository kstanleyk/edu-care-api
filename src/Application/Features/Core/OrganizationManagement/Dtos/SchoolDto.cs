using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record SchoolDto(Guid Id, string Name, string Code, SchoolType Type, SchoolMode Mode, Guid OrganizationId, Address? Address, DateTime CreatedOn);