using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record MarkEnrollmentInactiveCommand(Guid EnrollmentId) : IRequest;