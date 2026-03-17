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
        var trip = GetActiveTrip();

        var place = new Place(city, country);
        trip.AddStay(place, start, end);

        _repository.Update(trip);
    }

    public IReadOnlyList<StaySummary> GetStays()
    {
        var trip = GetActiveTrip();

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
        var trip = GetActiveTrip();
        return trip.TotalSpent();
    }

    public decimal GetTripRemainingBudget()
    {
        var trip = GetActiveTrip();
        return trip.RemainingBudget();
    }

    public void ArchiveActiveTrip()
    {
        var trip = GetActiveTrip();

        trip.Archive();
        _repository.Update(trip);
    }

    public void DeleteTrip(Guid tripId)
    {
        _repository.Delete(tripId);
    }

    public void UpdateStayStartDate(Guid stayId, DateTime startDate)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.SetStartDate(startDate);

        _repository.Update(trip);
    }

    public void UpdateStayEndDate(Guid stayId, DateTime endDate)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.SetEndDate(endDate);

        _repository.Update(trip);
    }

    public void DeleteStay(Guid stayId)
    {
        var trip = GetActiveTrip();

        trip.RemoveStay(stayId);

        _repository.Update(trip);
    }

    private Trip GetActiveTrip()
    {
        return _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");
    }

    private Stay GetStay(Guid stayId)
    {
        var trip = GetActiveTrip();

        return trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");
    }

    public void UpdateStayPlace(Guid stayId, string city, string country)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.SetPlace(new Place(city, country));
        _repository.Update(trip);
    }

    // Expenses
    public void AddExpenseToStay(Guid stayId, string name, decimal amount, ExpenseCategory category, string? note = null)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.AddExpense(name, amount, category, note);
        _repository.Update(trip);
    }

    public IReadOnlyList<ExpenseSummary> GetExpensesForStay(Guid stayId)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        return stay.Expenses
            .OrderBy(e => e.Name)
            .Select(e => new ExpenseSummary(
                e.Id,
                e.Name,
                e.Amount,
                e.Category,
                e.Notes,
                e.CreatedAt
            ))
            .ToList();
    }

    public void UpdateExpenseTitle(Guid stayId, Guid expenseId, string newName)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var expense = stay.GetExpense(expenseId);
        expense.Rename(newName);

        _repository.Update(trip);
    }

    public void UpdateExpenseAmount(Guid stayId, Guid expenseId, decimal newAmount)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var expense = stay.GetExpense(expenseId);
        expense.UpdateAmount(newAmount);

        _repository.Update(trip);
    }

    public void UpdateExpenseNotes(Guid stayId, Guid expenseId, string? newNotes)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var expense = stay.GetExpense(expenseId);
        expense.UpdateNotes(newNotes);

        _repository.Update(trip);
    }

    public void DeleteExpense(Guid stayId, Guid expenseId)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.RemoveExpense(expenseId);

        _repository.Update(trip);
    }


    // Bookmarks
    public void AddBookmarkToStay(Guid stayId, string title, string url, string? notes = null)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.AddBookmark(title, url, notes);

        _repository.Update(trip);
    }

    public IReadOnlyList<BookmarkSummary> GetBookmarksForStay(Guid stayId)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        return stay.Bookmarks
            .OrderBy(b => b.Title)
            .Select(b => new BookmarkSummary(
                b.Id,
                b.Title,
                b.Url,
                b.Notes,
                b.CreatedAt
            ))
            .ToList();
    }

    public void UpdateBookmarkTitle(Guid stayId, Guid bookmarkId, string newTitle)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var bookmark = stay.GetBookmark(bookmarkId);
        bookmark.Rename(newTitle);

        _repository.Update(trip);
    }

    public void UpdateBookmarkUrl(Guid stayId, Guid bookmarkId, string newUrl)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var bookmark = stay.GetBookmark(bookmarkId);
        bookmark.UpdateUrl(newUrl);

        _repository.Update(trip);
    }

    public void UpdateBookmarkNotes(Guid stayId, Guid bookmarkId, string? newNotes)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var bookmark = stay.GetBookmark(bookmarkId);
        bookmark.UpdateNotes(newNotes);

        _repository.Update(trip);
    }

    public void DeleteBookmark(Guid stayId, Guid bookmarkId)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.RemoveBookmark(bookmarkId);

        _repository.Update(trip);
    }

    // FlightOptions
    public void AddFlightOptionToStay(
        Guid stayId,
        string url,
        string fromAirportCode,
        string toAirportCode,
        DateTime departTime,
        DateTime arriveTime,
        decimal? price = null)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.AddFlightOption(url, fromAirportCode, toAirportCode, departTime, arriveTime, price);

        _repository.Update(trip);
    }

    public IReadOnlyList<FlightOptionSummary> GetFlightOptionsForStay(Guid stayId)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        return stay.FlightOptions
            .OrderBy(f => f.DepartTime)
            .Select(f => new FlightOptionSummary(
                f.Id,
                f.Url,
                f.Price,
                f.CreatedAt,
                f.LastCheckedAt,
                f.IsSelected,
                f.FromAirportCode,
                f.ToAirportCode,
                f.DepartTime,
                f.ArriveTime
            ))
            .ToList();
    }

    public void DeleteFlightOption(Guid stayId, Guid flightOptionId)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        stay.RemoveFlightOption(flightOptionId);

        _repository.Update(trip);
    }

    // LodgingOptions
    public void AddLodgingOptionToStay(
        Guid stayId,
        string url,
        string propertyName,
        DateTime checkInDate,
        DateTime checkOutDate,
        decimal? price = null)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        stay.AddLodgingOption(url, propertyName, checkInDate, checkOutDate, price);

        _repository.Update(trip);
    }

    public IReadOnlyList<LodgingOptionSummary> GetLodgingOptionsForStay(Guid stayId)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        return stay.LodgingOptions
            .OrderBy(l => l.CheckInDate)
            .Select(l => new LodgingOptionSummary(
                l.Id,
                l.Url,
                l.Price,
                l.CreatedAt,
                l.LastCheckedAt,
                l.IsSelected,
                l.PropertyName,
                l.CheckInDate,
                l.CheckOutDate
            ))
            .ToList();
    }

    public void DeleteLodgingOption(Guid stayId, Guid lodgingOptionId)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        stay.RemoveLodgingOption(lodgingOptionId);

        _repository.Update(trip);
    }
}