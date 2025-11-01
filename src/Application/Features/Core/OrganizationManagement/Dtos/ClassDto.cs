namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record ClassDto(Guid Id, string Name, string Code, int GradeLevel, Guid AcademicYearId, DateTime CreatedOn);