using MediatR;

namespace EduCare.Application.Features.Core.FeeManagement.Commands;

public record CreateFeeItemCommand(string Name, string Description, string Category, string Code) : IRequest<Guid>;