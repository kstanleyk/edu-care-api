using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record UpdateFeeItemCommand(Guid Id, string Name, string Description, string Category, bool IsActive) : IRequest;