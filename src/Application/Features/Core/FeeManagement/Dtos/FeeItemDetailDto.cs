using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.FeeManagement.Dtos;

public record FeeItemDetailDto(
    Guid FeeItemId,
    string Name,
    string Category,
    string Description,
    Money Amount,
    bool IsOptional,
    bool IsSelected,
    int DisplayOrder
);