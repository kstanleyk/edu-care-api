using MediatR;

namespace EduCare.Application.Features.Core.ScholarshipManagement.Commands;

public record RevokeScholarshipCommand(Guid ScholarshipId) : IRequest;