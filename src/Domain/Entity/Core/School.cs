using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class School : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public SchoolType Type { get; private set; }
    public SchoolMode Mode { get; private set; }
    public Address? Address { get; private set; }
    public Guid OrganizationId { get; private set; }

    private readonly List<AcademicYear> _academicYears = [];
    public IReadOnlyCollection<AcademicYear> AcademicYears => _academicYears.AsReadOnly();

    protected School() { }

    /// <summary>
    /// Creates a new school
    /// </summary>
    /// <param name="name">School name</param>
    /// <param name="code">Unique school code</param>
    /// <param name="type">School type (Primary/Secondary)</param>
    /// <param name="mode">School mode (Day/Boarding/Both)</param>
    /// <param name="organizationId">Parent organization ID</param>
    /// <param name="address">School address</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static School Create(string name, string code, SchoolType type, SchoolMode mode,
        Guid organizationId, Address? address = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(code, nameof(code));

        return new School
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Type = type,
            Mode = mode,
            OrganizationId = organizationId,
            Address = address,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(string name, SchoolType type, SchoolMode mode, Address? address)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name;
        Type = type;
        Mode = mode;
        Address = address;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddAcademicYear(AcademicYear academicYear)
    {
        DomainGuards.AgainstNull(academicYear, nameof(academicYear));
        _academicYears.Add(academicYear);
    }
}