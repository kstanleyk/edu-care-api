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

    /// <summary>
    /// Compares two Address objects for equality
    /// </summary>
    public static bool AddressEquals(Address? address1, Address? address2)
    {
        if (address1 is null && address2 is null)
            return true;
        if (address1 is null || address2 is null)
            return false;

        return address1.Street == address2.Street &&
               address1.City == address2.City &&
               address1.State == address2.State &&
               address1.Country == address2.Country &&
               address1.ZipCode == address2.ZipCode;
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