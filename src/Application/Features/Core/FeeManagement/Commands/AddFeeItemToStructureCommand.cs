using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record AddFeeItemToStructureCommand(Guid FeeStructureId, Guid FeeItemId, Money Amount, bool IsOptional = false, int DisplayOrder = 0) : IRequest;