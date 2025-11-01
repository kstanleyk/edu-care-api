namespace EduCare.Application.Features.Core.FeeManagement.Dtos;

public record FeeItemDto(Guid Id, string Name, string Description, string Category, string Code, bool IsActive, DateTime CreatedOn);