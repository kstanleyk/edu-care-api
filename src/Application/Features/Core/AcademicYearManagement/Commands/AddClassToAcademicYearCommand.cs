using MediatR;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Commands;

public record AddClassToAcademicYearCommand(Guid AcademicYearId, string Name, string Code, int GradeLevel)
    : IRequest<Guid>;