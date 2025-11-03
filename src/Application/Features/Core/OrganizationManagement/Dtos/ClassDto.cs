namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record ClassDto(Guid Id, string Name, string Code, int GradeLevel, Guid AcademicYearId,string AcademicYearName, string AcademicYearCode, DateTime CreatedOn, DateTime? ModifiedOn);