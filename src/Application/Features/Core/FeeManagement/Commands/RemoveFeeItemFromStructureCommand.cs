using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record RemoveFeeItemFromStructureCommand(Guid FeeStructureId, Guid FeeItemId) : IRequest;