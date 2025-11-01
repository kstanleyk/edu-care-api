namespace EduCare.Application.Features.Core.Dtos;

public record EnrollmentDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = null!;
    public Guid AcademicYearId { get; init; }
    public string AcademicYearName { get; init; } = null!;
    public Guid FeeStructureId { get; init; }
    public string FeeStructureName { get; init; } = null!;
    public DateOnly EnrollmentDate { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedOn { get; init; }

    // Parameterless constructor for AutoMapper
    public EnrollmentDto() { }
}