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
    public bool IsArchived { get; private set; }
    public bool WarnOnOverBudget { get; private set; }
    public string? HomeAirportCode { get; private set; }
    public string DefaultCurrency { get; private set; } = "USD";
    public int? TravelerCount { get; private set; }

    private readonly List<Stay> _stays = new();
    public IReadOnlyCollection<Stay> Stays => _stays.AsReadOnly();

    public Trip(string name, decimal totalBudget)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Trip name cannot be empty.", nameof(name));
        if (totalBudget < 0) throw new ArgumentException("Budget cannot be negative.", nameof(totalBudget));

        Id = Guid.NewGuid();
        Name = name.Trim();
        TotalBudget = totalBudget;
        WarnOnOverBudget = totalBudget > 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(string newName)
    {
        EnsureNotArchived();
        
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Trip name cannot be empty.");

        Name = newName.Trim();
    }

    public void UpdateBudget(decimal newBudget)
    {
        EnsureNotArchived();

        if (newBudget < 0) throw new ArgumentException("Budget cannot be negative.");
        if (TotalBudget == 0 && newBudget > 0)
            WarnOnOverBudget = true;
        TotalBudget = newBudget;
    }

    public void SetWarnOnOverBudget(bool warn)
    {
        EnsureNotArchived();
        WarnOnOverBudget = warn;
    }

    public void SetHomeAirportCode(string? code)
    {
        EnsureNotArchived();
        HomeAirportCode = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
    }

    public void SetDefaultCurrency(string currency)
    {
        EnsureNotArchived();
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));
        DefaultCurrency = currency.Trim().ToUpperInvariant();
    }

    public void SetTravelerCount(int? count)
    {
        EnsureNotArchived();
        if (count.HasValue && count.Value < 1)
            throw new ArgumentException("Traveler count must be at least 1.", nameof(count));
        TravelerCount = count;
    }

    // Route A: no uniqueness constraints. Add as many Tokyo stays as you want.
    public Stay AddStay(Place place, DateTime? start = null, DateTime? end = null)
    {
        EnsureNotArchived();
        
        var stay = (start.HasValue && end.HasValue)
            ? new Stay(place, start.Value, end.Value)
            : new Stay(place);

        _stays.Add(stay);
        return stay;
    }

    // Also allow for an add with a city and country
    public Stay AddStay(string city, string country, DateTime? start = null, DateTime? end = null)
    {
        EnsureNotArchived();

        var place = new Place(city, country);

        if (start.HasValue && end.HasValue)
            return AddStay(place, start.Value, end.Value);

        return AddStay(place);
    }

    public void RemoveStay(Guid stayId)
    {
        EnsureNotArchived();
        
        var stay = _stays.FirstOrDefault(s => s.Id == stayId);
        if (stay == null) throw new InvalidOperationException("Stay not found.");
        _stays.Remove(stay);
    }

    public decimal TotalExpenses() => _stays.Sum(s => s.TotalExpenses());

    public decimal TotalSelectedTravelOptionCost() => _stays.Sum(s => s.TotalSelectedTravelOptionCost());

    public decimal TotalPlannedCost() => _stays.Sum(s => s.TotalPlannedCost());

    public decimal RemainingBudget() => TotalBudget - TotalPlannedCost();

    internal static Trip Hydrate(Guid id, string name, decimal totalBudget, DateTime createdAt, bool warnOnOverBudget,
        string? homeAirportCode = null, string? defaultCurrency = null, int? travelerCount = null)
    {
        var trip = new Trip(name, totalBudget);
        trip.Id = id;
        trip.CreatedAt = createdAt;
        trip.WarnOnOverBudget = warnOnOverBudget;
        trip.HomeAirportCode = string.IsNullOrWhiteSpace(homeAirportCode) ? null : homeAirportCode.Trim().ToUpperInvariant();
        trip.DefaultCurrency = string.IsNullOrWhiteSpace(defaultCurrency) ? "USD" : defaultCurrency.Trim().ToUpperInvariant();
        trip.TravelerCount = travelerCount;
        return trip;
    }

    internal void HydrateAddStay(Stay stay)
    {
        _stays.Add(stay);
    }

    public void Archive()
    {
        IsArchived = true;
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
            throw new InvalidOperationException("Archived trips cannot be modified.");
    }
}