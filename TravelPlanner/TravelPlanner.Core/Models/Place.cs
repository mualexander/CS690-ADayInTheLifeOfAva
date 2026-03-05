using System;

namespace TravelPlanner.Core.Models;

public class Place
{
    public Guid Id { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    public Place(string city, string country)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        Id = Guid.NewGuid();
        City = Normalize(city);
        Country = Normalize(country);
    }

    public string DisplayName => $"{City}, {Country}";

    private static string Normalize(string s) => s.Trim();

    public override string ToString() => DisplayName;
}