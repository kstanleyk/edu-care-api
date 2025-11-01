using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.FeeManagement.Dtos;

public record FeeStructureItemDto(Guid Id, Guid FeeItemId, Money Amount, bool IsOptional, int DisplayOrder, string FeeItemName, string FeeItemCategory);