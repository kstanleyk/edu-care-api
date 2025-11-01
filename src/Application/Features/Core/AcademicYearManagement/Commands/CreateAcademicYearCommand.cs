using MediatR;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Commands;

public record CreateAcademicYearCommand(string Name, string Code, DateOnly StartDate, DateOnly EndDate, Guid SchoolId, bool IsCurrent = false) : IRequest<Guid>;