using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record SelectOptionalFeeCommand(Guid EnrollmentId, Guid FeeItemId) : IRequest;