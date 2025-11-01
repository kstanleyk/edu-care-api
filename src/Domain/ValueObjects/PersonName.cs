using EduCare.Domain.Entity;

namespace EduCare.Domain.ValueObjects;

public class PersonName : ValueObject
{
    public string FirstName { get; }
    public string? MiddleName { get; }
    public string LastName { get; }

    private PersonName() { }

    public PersonName(string firstName, string lastName, string? middleName = null)
    {
        DomainGuards.AgainstNullOrWhiteSpace(firstName, nameof(firstName));
        DomainGuards.AgainstNullOrWhiteSpace(lastName, nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }

    public string FullName => MiddleName is null
        ? $"{FirstName} {LastName}"
        : $"{FirstName} {MiddleName} {LastName}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        if (MiddleName is not null) yield return MiddleName;
    }
}