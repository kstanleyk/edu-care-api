using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.FeeManagement.Dtos;

public record FeeStructureDto(
    Guid Id,
    string Name,
    string Description,
    Guid ClassId,
    string ClassName,
    string ClassCode,
    bool IsActive,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    Money TotalMandatoryFees,
    Money TotalWithOptionalFees,
    List<FeeStructureItemDto> FeeItems
);