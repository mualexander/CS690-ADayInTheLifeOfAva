namespace TravelPlanner.Core.Models;

public class Place
{
    public Guid Id { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    public Place(string city, string country)
    {
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City cannot be empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country cannot be empty.", nameof(country));

        Id = Guid.NewGuid();
        City = city.Trim();
        Country = country.Trim();
    }
}