namespace EduCare.Application.Features.Core.OrganizationManagement.Dtos;

public record EnrollmentDto(Guid Id, Guid StudentId, Guid ClassId, Guid AcademicYearId, Guid FeeStructureId, DateOnly EnrollmentDate, bool IsActive, DateTime CreatedOn);