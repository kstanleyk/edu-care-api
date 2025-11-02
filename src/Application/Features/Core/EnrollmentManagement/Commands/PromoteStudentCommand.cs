using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record PromoteStudentCommand(
    Guid EnrollmentId,
    Guid NextClassId,
    Guid NextAcademicYearId,
    Guid NewFeeStructureId) : IRequest<Guid>;