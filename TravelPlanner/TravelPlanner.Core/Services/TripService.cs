using System;
using System.Linq;
using TravelPlanner.Core.Interfaces;
using TravelPlanner.Core.Models;

namespace TravelPlanner.Core.Services;

public class TripService
{
    private readonly ITripRepository _repository;
    private readonly ITripContext _context;

    public TripService(ITripRepository repository, ITripContext context)
    {
        _repository = repository;
        _context = context;
    }

    public Trip CreateTrip(string name, decimal budget)
    {
        var trip = new Trip(name, budget);
        _repository.Add(trip);
        return trip;
    }

    public void SelectTrip(Guid tripId) => _context.SetActiveTrip(tripId);

    public void AddLocation(string name, string country)
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");
        trip.AddLocation(new Location(name, country));
        _repository.Update(trip);
    }

    // expose locations to the UI as summaries (includes Id for selection)
    public IReadOnlyList<LocationSummary> GetLocations()
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");

        return trip.Locations
            .Select(l => new LocationSummary(l.Id, l.Name, l.Country, l.TotalSpent()))
            .ToList();
    }

    public IReadOnlyList<TripSummary> GetTrips()
    {
        return _repository.GetAll()
            .Select(t => new TripSummary(
                t.Id,
                t.Name,
                t.TotalBudget,
                t.TotalSpent(),
                t.RemainingBudget(),
                t.Locations.Count
            ))
            .ToList();
    }

    // NEW: add expense by locationId chosen from GetLocations()
    public void AddExpenseToLocation(Guid locationId, DateTime date, decimal amount, ExpenseCategory category, string? note = null)
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");

        var location = trip.Locations.FirstOrDefault(l => l.Id == locationId)
            ?? throw new InvalidOperationException("Location not found.");

        location.AddExpense(date, amount, category, note);
        _repository.Update(trip);
    }

    public decimal GetTripTotalSpent()
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");
        return trip.TotalSpent();
    }

    public decimal GetTripRemainingBudget()
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");
        return trip.RemainingBudget();
    }
}