using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record CreateFeeStructureCommand(string Name, string Description, Guid ClassId, DateTime EffectiveFrom, DateTime? EffectiveTo) : IRequest<Guid>;