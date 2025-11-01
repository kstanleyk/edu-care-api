using EduCare.Domain.Abstractions;

namespace EduCare.Domain.Entity.Core;

public class AcademicYear : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public bool IsCurrent { get; private set; }
    public Guid SchoolId { get; private set; }

    private readonly List<Class> _classes = [];
    public IReadOnlyCollection<Class> Classes => _classes.AsReadOnly();

    protected AcademicYear() { }

    /// <summary>
    /// Creates a new academic year
    /// </summary>
    /// <param name="name">Academic year name</param>
    /// <param name="code">Unique code for the academic year</param>
    /// <param name="startDate">Academic year start date</param>
    /// <param name="endDate">Academic year end date</param>
    /// <param name="schoolId">Parent school ID</param>
    /// <param name="isCurrent">Whether this is the current academic year</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static AcademicYear Create(string name, string code, DateOnly startDate, DateOnly endDate,
        Guid schoolId, bool isCurrent = false, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(code, nameof(code));

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        return new AcademicYear
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            StartDate = startDate,
            EndDate = endDate,
            IsCurrent = isCurrent,
            SchoolId = schoolId,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(string name, DateOnly startDate, DateOnly endDate, bool isCurrent)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        IsCurrent = isCurrent;
        ModifiedOn = DateTime.UtcNow;
    }

    public void MarkAsCurrent()
    {
        IsCurrent = true;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddClass(Class @class)
    {
        DomainGuards.AgainstNull(@class, nameof(@class));
        _classes.Add(@class);
    }
}