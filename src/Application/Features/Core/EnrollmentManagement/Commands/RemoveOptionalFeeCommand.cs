using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record RemoveOptionalFeeCommand(Guid EnrollmentId, Guid FeeItemId) : IRequest;