using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class Organization : Aggregate<Guid>
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public Address? Address { get; private set; }

    private readonly List<School> _schools = [];
    public IReadOnlyCollection<School> Schools => _schools.AsReadOnly();

    protected Organization() { }

    /// <summary>
    /// Creates a new organization
    /// </summary>
    /// <param name="name">Organization name</param>
    /// <param name="code">Unique organization code</param>
    /// <param name="address">Organization address</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static Organization Create(string name, string code, Address? address = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(code, nameof(code));

        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Address = address,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(string name, Address? address)
    {
        DomainGuards.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name;
        Address = address;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddSchool(School school)
    {
        DomainGuards.AgainstNull(school, nameof(school));
        _schools.Add(school);
    }

    public void RemoveSchool(School school)
    {
        DomainGuards.AgainstNull(school, nameof(school));
        _schools.Remove(school);
    }
}