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

        if (_repository.GetAll().Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Trip with that name already exists.");
        }
        _repository.Add(trip);
        return trip;
    }

    public void SelectTrip(Guid tripId) => _context.SetActiveTrip(tripId);

    public void AddStay(string city, string country, DateTime? start = null, DateTime? end = null)
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");

        var place = new Place(city, country);
        trip.AddStay(place, start, end);

        _repository.Update(trip);
    }

    public IReadOnlyList<StaySummary> GetStays()
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");

        return trip.Stays
            .Select(s => new StaySummary(
                s.Id,
                s.DisplayKey,
                s.Place.City,
                s.Place.Country,
                s.StartDate,
                s.EndDate,
                s.TotalSpent()
            ))
            .ToList();
    }

    public void AddExpenseToStay(Guid stayId, DateTime date, decimal amount, ExpenseCategory category, string? note = null)
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        stay.AddExpense(date, amount, category, note);
        _repository.Update(trip);
    }

    public IReadOnlyList<TripSummary> GetTrips()
    {
        return _repository.GetAll().Where(t => !t.IsArchived)
            .Select(t => new TripSummary(
                t.Id,
                t.Name,
                t.TotalBudget,
                t.TotalSpent(),
                t.RemainingBudget(),
                t.Stays.Count
            ))
            .ToList();
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

    public void ArchiveActiveTrip()
    {
        var trip = _context.ActiveTrip ?? throw new InvalidOperationException("No active trip.");

        trip.Archive();
        _repository.Update(trip);
    }

    public void DeleteTrip(Guid tripId)
    {
        _repository.Delete(tripId);
    }

    public void UpdateStayStartDate(Guid stayId, DateTime startDate)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        stay.SetStartDate(startDate);

        _repository.Update(trip);
    }

    public void UpdateStayEndDate(Guid stayId, DateTime endDate)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        stay.SetEndDate(endDate);

        _repository.Update(trip);
    }

    public void DeleteStay(Guid stayId)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        trip.RemoveStay(stayId);

        _repository.Update(trip);
    }

    private Stay GetStay(Guid stayId)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        return trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");
    }

    public void UpdateStayPlace(Guid stayId, string city, string country)
    {
        var trip = _context.ActiveTrip!;
        var stay = GetStay(stayId);

        stay.SetPlace(new Place(city, country));
        _repository.Update(trip);
    }
}