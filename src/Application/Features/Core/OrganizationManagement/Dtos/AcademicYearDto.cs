namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record AcademicYearDto(
    Guid Id,
    string Name,
    string Code,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    Guid SchoolId,
    DateTime CreatedOn,
    DateTime? ModifiedOn
);
