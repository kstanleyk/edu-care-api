using EduCare.Domain.Abstractions;

namespace EduCare.Domain.Entity.Core;

public class Class : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public int GradeLevel { get; private set; }
    public Guid AcademicYearId { get; private set; }

    protected Class() { }

    /// <summary>
    /// Creates a new class
    /// </summary>
    /// <param name="name">Class name</param>
    /// <param name="code">Unique class code</param>
    /// <param name="gradeLevel">Grade level</param>
    /// <param name="academicYearId">Parent academic year ID</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static Class Create(string name, string code, int gradeLevel, Guid academicYearId,
        DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(code, nameof(code));

        return new Class
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            GradeLevel = gradeLevel,
            AcademicYearId = academicYearId,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(string name, int gradeLevel)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name;
        GradeLevel = gradeLevel;
        ModifiedOn = DateTime.UtcNow;
    }
}