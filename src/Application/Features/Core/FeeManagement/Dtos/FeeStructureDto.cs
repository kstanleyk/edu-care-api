namespace EduCare.Application.Features.Core.FeeManagement.Dtos;

public record FeeStructureDto(Guid Id, string Name, string Description, Guid ClassId, bool IsActive, DateTime EffectiveFrom, DateTime? EffectiveTo, DateTime CreatedOn, List<FeeStructureItemDto> FeeItems);