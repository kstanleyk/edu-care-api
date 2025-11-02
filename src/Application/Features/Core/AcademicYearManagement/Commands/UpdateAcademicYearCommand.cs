using MediatR;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Commands;

public record UpdateAcademicYearCommand(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, bool IsCurrent)
    : IRequest;