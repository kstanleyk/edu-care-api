using EduCare.Domain.Abstractions;
using EduCare.Domain.ValueObjects;

namespace EduCare.Domain.Entity.Core;

public class Parent : Aggregate<Guid>
{
    public PersonName Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public Address? Address { get; private set; }
    public string Relationship { get; private set; } = null!;
    public bool IsPrimaryContact { get; private set; }

    private readonly List<Student> _students = [];
    public IReadOnlyCollection<Student> Students => _students.AsReadOnly();

    protected Parent() { }

    /// <summary>
    /// Creates a new parent/guardian
    /// </summary>
    /// <param name="name">Parent's name</param>
    /// <param name="email">Parent's email</param>
    /// <param name="phone">Parent's phone number</param>
    /// <param name="relationship">Relationship to student</param>
    /// <param name="isPrimaryContact">Whether this is the primary contact</param>
    /// <param name="address">Parent's address</param>
    /// <param name="createdOn">Creation timestamp</param>
    public static Parent Create(PersonName name, string email, string phone, string relationship,
        bool isPrimaryContact = false, Address? address = null, DateTime? createdOn = null)
    {
        DomainGuards.AgainstNull(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(email, nameof(email));
        DomainGuards.AgainstNullOrWhiteSpace(phone, nameof(phone));
        DomainGuards.AgainstNullOrWhiteSpace(relationship, nameof(relationship));

        return new Parent
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Phone = phone,
            Relationship = relationship,
            IsPrimaryContact = isPrimaryContact,
            Address = address,
            CreatedOn = createdOn ?? DateTime.UtcNow
        };
    }

    public void Update(PersonName name, string email, string phone, string relationship, Address? address)
    {
        DomainGuards.AgainstNull(name, nameof(name));
        DomainGuards.AgainstNullOrWhiteSpace(email, nameof(email));
        DomainGuards.AgainstNullOrWhiteSpace(phone, nameof(phone));
        DomainGuards.AgainstNullOrWhiteSpace(relationship, nameof(relationship));

        Name = name;
        Email = email;
        Phone = phone;
        Relationship = relationship;
        Address = address;
        ModifiedOn = DateTime.UtcNow;
    }

    public void SetAsPrimaryContact()
    {
        IsPrimaryContact = true;
        ModifiedOn = DateTime.UtcNow;
    }

    public void AddStudent(Student student)
    {
        DomainGuards.AgainstNull(student, nameof(student));
        _students.Add(student);
    }

    public void RemoveStudent(Student student)
    {
        DomainGuards.AgainstNull(student, nameof(student));
        _students.Remove(student);
    }
}