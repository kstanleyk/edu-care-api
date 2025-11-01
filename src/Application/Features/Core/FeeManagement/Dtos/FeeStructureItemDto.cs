using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.FeeManagement.Dtos;

public record FeeStructureItemDto(
    Guid Id,
    Guid FeeItemId,
    string FeeItemName,
    string FeeItemDescription,
    string FeeItemCategory,
    string FeeItemCode,
    Money Amount,
    bool IsOptional,
    int DisplayOrder
);