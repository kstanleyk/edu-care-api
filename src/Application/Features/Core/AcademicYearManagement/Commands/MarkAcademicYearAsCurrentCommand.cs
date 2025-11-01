using MediatR;

namespace EduCare.Application.Features.Core.AcademicYearManagement.Commands;

public record MarkAcademicYearAsCurrentCommand(Guid Id) : IRequest;