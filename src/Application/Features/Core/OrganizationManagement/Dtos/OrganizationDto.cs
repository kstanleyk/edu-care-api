using EduCare.Domain.ValueObjects;

namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record OrganizationDto(
    Guid Id,
    string Name,
    string Code,
    Address? Address,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    List<SchoolSummaryDto> Schools
);

public record SchoolSummaryDto(
    Guid Id,
    string Name,
    string Code,
    SchoolType Type,
    SchoolMode Mode,
    Address? Address,
    DateTime CreatedOn
);