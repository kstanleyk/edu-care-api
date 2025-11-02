using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record UpdateFeeStructureCommand(
    Guid Id,
    string Name,
    string Description,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsActive) : IRequest;