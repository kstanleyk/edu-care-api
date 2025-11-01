using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record UpdateFeeItemAmountCommand(Guid FeeStructureId, Guid FeeItemId, Money NewAmount) : IRequest;