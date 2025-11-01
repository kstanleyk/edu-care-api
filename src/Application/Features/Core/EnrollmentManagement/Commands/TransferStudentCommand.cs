using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record TransferStudentCommand(Guid EnrollmentId, Guid NewClassId, Guid NewFeeStructureId) : IRequest;