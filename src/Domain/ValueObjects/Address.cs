using EduCare.Domain.Entity;

namespace EduCare.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }
    public string ZipCode { get; }

    private Address() { }

    public Address(string street, string city, string state, string country, string zipCode)
    {
        DomainGuards.AgainstNullOrWhiteSpace(street, nameof(street));
        DomainGuards.AgainstNullOrWhiteSpace(city, nameof(city));
        DomainGuards.AgainstNullOrWhiteSpace(state, nameof(state));
        DomainGuards.AgainstNullOrWhiteSpace(country, nameof(country));
        DomainGuards.AgainstNullOrWhiteSpace(zipCode, nameof(zipCode));

        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return ZipCode;
    }
}