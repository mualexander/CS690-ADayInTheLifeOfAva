using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelPlanner.Core.Models;

public class Trip
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal TotalBudget { get; private set; }

    private readonly List<Location> _locations = new();
    public IReadOnlyCollection<Location> Locations => _locations.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public Trip(string name, decimal totalBudget)
    {
        Id = Guid.NewGuid();
        Name = name;
        TotalBudget = totalBudget;
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Trip name cannot be empty.");

        Name = newName;
    }

    public void UpdateBudget(decimal newBudget)
    {
        if (newBudget < 0)
            throw new ArgumentException("Budget cannot be negative.");

        TotalBudget = newBudget;
    }

    public void AddLocation(Location location)
    {
        if (_locations.Any(l => l.Name == location.Name))
            throw new InvalidOperationException("Location already exists.");

        _locations.Add(location);
    }

    public void RemoveLocation(Guid locationId)
    {
        var location = _locations.FirstOrDefault(l => l.Id == locationId);
        if (location == null)
            throw new InvalidOperationException("Location not found.");

        _locations.Remove(location);
    }

    public decimal TotalSpent() => Locations.Sum(l => l.TotalSpent());

    public decimal RemainingBudget() => TotalBudget - TotalSpent();
}