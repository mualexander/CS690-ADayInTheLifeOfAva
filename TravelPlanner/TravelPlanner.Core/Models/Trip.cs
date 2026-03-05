using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelPlanner.Core.Models;

public class Trip
{
    public Guid Id { get; internal set; }
    public string Name { get; private set; }
    public decimal TotalBudget { get; private set; }
    public DateTime CreatedAt { get; internal set; }

    private readonly List<Stay> _stays = new();
    public IReadOnlyCollection<Stay> Stays => _stays.AsReadOnly();

    public Trip(string name, decimal totalBudget)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Trip name cannot be empty.", nameof(name));
        if (totalBudget < 0) throw new ArgumentException("Budget cannot be negative.", nameof(totalBudget));

        Id = Guid.NewGuid();
        Name = name.Trim();
        TotalBudget = totalBudget;
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Trip name cannot be empty.");

        Name = newName.Trim();
    }

    public void UpdateBudget(decimal newBudget)
    {
        if (newBudget < 0) throw new ArgumentException("Budget cannot be negative.");
        TotalBudget = newBudget;
    }

    // Route A: no uniqueness constraints. Add as many Tokyo stays as you want.
    public Stay AddStay(Place place, DateTime? start = null, DateTime? end = null)
    {
        var stay = (start.HasValue && end.HasValue)
            ? new Stay(place, start.Value, end.Value)
            : new Stay(place);

        _stays.Add(stay);
        return stay;
    }

    // Also allow for an add with a city and country
    public Stay AddStay(string city, string country, DateTime? start = null, DateTime? end = null)
    {
        var place = new Place(city, country);

        if (start.HasValue && end.HasValue)
            return AddStay(place, start.Value, end.Value);

        return AddStay(place);
    }

    public void RemoveStay(Guid stayId)
    {
        var stay = _stays.FirstOrDefault(s => s.Id == stayId);
        if (stay == null) throw new InvalidOperationException("Stay not found.");
        _stays.Remove(stay);
    }

    public decimal TotalSpent() => _stays.Sum(s => s.TotalSpent());
    public decimal RemainingBudget() => TotalBudget - TotalSpent();

    internal static Trip Hydrate(Guid id, string name, decimal totalBudget, DateTime createdAt)
    {
        var trip = new Trip(name, totalBudget);
        trip.Id = id;
        trip.CreatedAt = createdAt;
        return trip;
    }

    internal void HydrateAddStay(Stay stay)
    {
        _stays.Add(stay);
    }
}