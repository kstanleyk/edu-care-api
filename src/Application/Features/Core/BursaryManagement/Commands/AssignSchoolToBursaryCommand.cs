using MediatR;

namespace EduCare.Application.Features.Core.BursaryManagement.Commands;

public record AssignSchoolToBursaryCommand(Guid BursaryId, Guid SchoolId) : IRequest;