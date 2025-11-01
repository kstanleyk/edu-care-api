using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record OrganizationDto(Guid Id, string Name, string Code, Address? Address, DateTime CreatedOn);