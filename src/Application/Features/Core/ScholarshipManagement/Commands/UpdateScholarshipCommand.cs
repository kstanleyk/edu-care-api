using EduCare.Domain.ValueObjects;
using MediatR;

namespace EduCare.Application.Features.Core.ScholarshipManagement.Commands;

public record UpdateScholarshipCommand(
    Guid ScholarshipId,
    ScholarshipType Type,
    decimal Percentage,
    string Description,
    bool IsActive) : IRequest;