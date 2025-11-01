using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.ScholarshipManagement.Commands;

public record ApplyScholarshipCommand(Guid EnrollmentId, ScholarshipType Type, decimal Percentage, string Description) : IRequest<Guid>;