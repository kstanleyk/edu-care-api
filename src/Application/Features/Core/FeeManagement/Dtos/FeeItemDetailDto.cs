using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.FeeManagement.Dtos;

public record FeeItemDetailDto
{
    public Guid FeeItemId { get; init; }
    public string Name { get; init; } = null!;
    public string Category { get; init; } = null!;
    public string Description { get; init; } = null!;
    public Money Amount { get; init; } = null!;
    public bool IsOptional { get; init; }
    public bool IsSelected { get; init; }
    public int DisplayOrder { get; init; }
}