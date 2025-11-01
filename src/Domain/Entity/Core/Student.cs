using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class Student : Aggregate<Guid>
{
    public string StudentId { get; private set; } = null!;
    public PersonName Name { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }
    public string? Gender { get; private set; }

    private readonly List<Parent> _parents = [];
    public IReadOnlyCollection<Parent> Parents => _parents.AsReadOnly();

    private readonly List<Enrollment> _enrollments = [];
    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    protected Student() { }

    /// <summary>
    /// Creates a new student with a unique ID
    /// </summary>
    /// <param name="studentId">Unique student identifier</param>
    /// <param name="name">Student's name</param>
    /// <param name="dateOfBirth">Student's date of birth</param>
    /// <param name="gender">Student's gender</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static Student Create(string studentId, PersonName name, DateOnly dateOfBirth,
        string? gender = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(studentId, nameof(studentId));
        DomainGuards.AgainstNull(name, nameof(name));

        return new Student
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            Name = name,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(PersonName name, DateOnly dateOfBirth, string? gender)
    {
        DomainGuards.AgainstNull(name, nameof(name));

        Name = name;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddParent(Parent parent)
    {
        DomainGuards.AgainstNull(parent, nameof(parent));
        _parents.Add(parent);
    }

    public void Enroll(Enrollment enrollment)
    {
        DomainGuards.AgainstNull(enrollment, nameof(enrollment));

        // Business rule: A student cannot be enrolled in the same academic year twice
        if (_enrollments.Any(e => e.AcademicYearId == enrollment.AcademicYearId && e.IsActive))
            throw new InvalidOperationException("Student is already enrolled in this academic year");

        _enrollments.Add(enrollment);
    }

    public Enrollment? GetCurrentEnrollment()
    {
        return _enrollments.FirstOrDefault(e => e.IsActive);
    }
}