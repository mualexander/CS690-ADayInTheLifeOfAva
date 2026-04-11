using System;
using System.Collections.Generic;
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
                s.TotalExpenses(),
                s.TotalSelectedFlightCost(),
                s.TotalSelectedLodgingCost(),
                s.TotalPlannedCost()
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
                t.TotalExpenses(),
                t.TotalSelectedTravelOptionCost(),
                t.TotalPlannedCost(),
                t.RemainingBudget(),
                t.Stays.Count
            ))
            .ToList();
    }

    public decimal GetTripTotalExpenses()
    {
        var trip = GetActiveTrip();
        return trip.TotalExpenses();
    }

    public decimal GetTripTotalTravelOptionCost()
    {
        var trip = GetActiveTrip();
        return trip.TotalSelectedTravelOptionCost();
    }

    public decimal GetTripTotalPlannedCost()
    {
        var trip = GetActiveTrip();
        return trip.TotalPlannedCost();
    }

    public decimal GetTripRemainingBudget()
    {
        var trip = GetActiveTrip();
        return trip.RemainingBudget();
    }

    public IReadOnlyList<CostItemSummary> GetTopCostItems(int count)
    {
        var trip = GetActiveTrip();
        var items = new List<CostItemSummary>();

        foreach (var stay in trip.Stays)
        {
            foreach (var e in stay.Expenses)
                items.Add(new CostItemSummary(stay.DisplayKey, "Expense", $"{e.Category} - {e.Name}", e.Amount));

            foreach (var f in stay.FlightOptions.Where(f => f.IsSelected && f.Price.HasValue))
                items.Add(new CostItemSummary(stay.DisplayKey, "Flight", $"{f.FromAirportCode}->{f.ToAirportCode}", f.Price!.Value));

            foreach (var l in stay.LodgingOptions.Where(l => l.IsSelected && l.Price.HasValue))
                items.Add(new CostItemSummary(stay.DisplayKey, "Lodging", l.PropertyName, l.Price!.Value));
        }

        return items.OrderByDescending(i => i.Price).Take(count).ToList();
    }

    public bool GetWarnOnOverBudget()
    {
        return GetActiveTrip().WarnOnOverBudget;
    }

    public bool IsOverBudget()
    {
        var trip = GetActiveTrip();
        return trip.TotalBudget > 0 && trip.TotalPlannedCost() > trip.TotalBudget;
    }

    public void SetWarnOnOverBudget(bool warn)
    {
        var trip = GetActiveTrip();
        trip.SetWarnOnOverBudget(warn);
        _repository.Update(trip);
    }

    public void UpdateTripBudget(decimal newBudget)
    {
        var trip = GetActiveTrip();
        trip.UpdateBudget(newBudget);
        _repository.Update(trip);
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
    public void AddBookmarkToStay(Guid stayId, string title, string url, string? notes = null, IEnumerable<string>? tags = null)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var bookmark = stay.AddBookmark(title, url, notes);
        if (tags != null)
            foreach (var tag in tags)
                bookmark.AddTag(tag);

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
                b.CreatedAt,
                b.Tags
            ))
            .ToList();
    }

    public void AddTagToBookmark(Guid stayId, Guid bookmarkId, string tag)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);
        stay.GetBookmark(bookmarkId).AddTag(tag);
        _repository.Update(trip);
    }

    public void RemoveTagFromBookmark(Guid stayId, Guid bookmarkId, string tag)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);
        stay.GetBookmark(bookmarkId).RemoveTag(tag);
        _repository.Update(trip);
    }

    public IReadOnlyCollection<string> GetAllTagsForActiveTrip()
    {
        var trip = GetActiveTrip();
        return trip.Stays
            .SelectMany(s => s.Bookmarks)
            .SelectMany(b => b.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    public IReadOnlyCollection<string> GetAllTagsAcrossAllTrips()
    {
        return _repository.GetAll()
            .Where(t => !t.IsArchived)
            .SelectMany(t => t.Stays)
            .SelectMany(s => s.Bookmarks)
            .SelectMany(b => b.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    public IReadOnlyList<TagSearchResultSummary> FindBookmarksByTag(string tag)
    {
        var normalized = tag.Trim().ToLowerInvariant();
        return _repository.GetAll()
            .Where(t => !t.IsArchived)
            .SelectMany(t => t.Stays.Select(s => (Trip: t, Stay: s)))
            .SelectMany(ts => ts.Stay.Bookmarks
                .Where(b => b.Tags.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                .Select(b => new TagSearchResultSummary(
                    ts.Trip.Name,
                    ts.Stay.DisplayKey,
                    b.Title,
                    b.Url,
                    b.Notes,
                    b.Tags)))
            .OrderBy(r => r.TripName)
            .ThenBy(r => r.StayDisplayKey)
            .ThenBy(r => r.Title)
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

    public void UpdateFlightOptionPrice(Guid stayId, Guid flightOptionId, decimal? newPrice)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var option = stay.GetFlightOption(flightOptionId);
        option.UpdatePrice(newPrice);

        _repository.Update(trip);
    }

    public void UpdateFlightOptionUrl(Guid stayId, Guid flightOptionId, string newUrl)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var option = stay.GetFlightOption(flightOptionId);
        option.UpdateUrl(newUrl);

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

    public void UpdateLodgingOptionPrice(Guid stayId, Guid lodgingOptionId, decimal? newPrice)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var option = stay.GetLodgingOption(lodgingOptionId);
        option.UpdatePrice(newPrice);

        _repository.Update(trip);
    }

    public void UpdateLodgingOptionUrl(Guid stayId, Guid lodgingOptionId, string newUrl)
    {
        var trip = GetActiveTrip();
        var stay = GetStay(stayId);

        var option = stay.GetLodgingOption(lodgingOptionId);
        option.UpdateUrl(newUrl);

        _repository.Update(trip);
    }

    public void SelectFlightOption(Guid stayId, Guid flightOptionId)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        var option = stay.GetFlightOption(flightOptionId);
        option.Select();

        _repository.Update(trip);
    }

    public void DeselectFlightOption(Guid stayId, Guid flightOptionId)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        var option = stay.GetFlightOption(flightOptionId);
        option.Deselect();

        _repository.Update(trip);
    }

    public void SelectLodgingOption(Guid stayId, Guid lodgingOptionId)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        var option = stay.GetLodgingOption(lodgingOptionId);
        option.Select();

        _repository.Update(trip);
    }

    public void DeselectLodgingOption(Guid stayId, Guid lodgingOptionId)
    {
        var trip = _context.ActiveTrip
            ?? throw new InvalidOperationException("No active trip.");

        var stay = trip.Stays.FirstOrDefault(s => s.Id == stayId)
            ?? throw new InvalidOperationException("Stay not found.");

        var option = stay.GetLodgingOption(lodgingOptionId);
        option.Deselect();

        _repository.Update(trip);
    }
}