using MediatR;

namespace EduCare.Application.Features.Core.SchoolManagement.Commands;

public record AddAcademicYearToSchoolCommand(Guid SchoolId, string Name, string Code, DateOnly StartDate, DateOnly EndDate, bool IsCurrent = false) : IRequest<Guid>;