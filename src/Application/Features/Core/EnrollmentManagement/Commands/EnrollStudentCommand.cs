using MediatR;

namespace EduCare.Application.Features.Core.EnrollmentManagement.Commands;

public record EnrollStudentCommand(Guid StudentId, Guid ClassId, Guid AcademicYearId, Guid FeeStructureId, DateOnly EnrollmentDate) : IRequest<Guid>;