using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelPlanner.Core.Models;

public class Stay
{
    public Guid Id { get; internal set; }
    public Place Place { get; private set; }

    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    private readonly List<Expense> _expenses = new();
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    private readonly List<Bookmark> _bookmarks = new();
    public IReadOnlyCollection<Bookmark> Bookmarks => _bookmarks.AsReadOnly();

    private readonly List<FlightOption> _flightOptions = new();
    public IReadOnlyCollection<FlightOption> FlightOptions => _flightOptions.AsReadOnly();

    private readonly List<LodgingOption> _lodgingOptions = new();
    public IReadOnlyCollection<LodgingOption> LodgingOptions => _lodgingOptions.AsReadOnly();

    public StayStatus Status { get; private set; } = StayStatus.Idea;

    public Stay(Place place)
    {
        Place = place ?? throw new ArgumentNullException(nameof(place));
        Id = Guid.NewGuid();
    }

    public Stay(Place place, DateTime start, DateTime end) : this(place)
    {
        SetDates(start, end);
    }

    public void SetDates(DateTime start, DateTime end)
    {
        // Treat dates as date-only (strip time)
        start = start.Date;
        end = end.Date;

        if (end < start)
            throw new ArgumentException("End date must be on or after start date.");

        StartDate = start;
        EndDate = end;
    }

    public void SetStartDate(DateTime startDate)
    {
        startDate = startDate.Date;

        if (EndDate.HasValue && startDate > EndDate.Value.Date)
            throw new ArgumentException("Start date cannot be after end date.");

        StartDate = startDate;
    }

    public void SetEndDate(DateTime endDate)
    {
        endDate = endDate.Date;

        if (StartDate.HasValue && endDate < StartDate.Value.Date)
            throw new ArgumentException("End date cannot be before start date.");

        EndDate = endDate;
    }

    public void ClearDates()
    {
        StartDate = null;
        EndDate = null;
    }

    public bool HasDates => StartDate.HasValue && EndDate.HasValue;

    public int? Days
    {
        get
        {
            if (!HasDates) return null;

            // Inclusive days: start=end => 1 day
            var days = (EndDate!.Value.Date - StartDate!.Value.Date).Days + 1;
            return Math.Max(1, days);
        }
    }

    public int? Nights
    {
        get
        {
            if (!HasDates) return null;

            // Hotel-style nights: a 1-day stay is 0 nights
            return Math.Max(0, Days!.Value - 1);
        }
    }

    public decimal TotalExpenses() => _expenses.Sum(e => e.Amount);

    public decimal TotalExpenses(ExpenseCategory category) =>
        _expenses.Where(e => e.Category == category).Sum(e => e.Amount);

    public decimal TotalPlannedCost() => TotalExpenses() + TotalSelectedTravelOptionCost();

    public string Label
    {
        get
        {
            if (!HasDates) return Place.DisplayName;

            var daysPart = Days.HasValue ? $" | {Days} day(s)" : "";
            var nightsPart = Nights.HasValue ? $" | {Nights} night(s)" : "";

            return $"{Place.DisplayName} ({StartDate:yyyy-MM-dd} → {EndDate:yyyy-MM-dd}){daysPart}{nightsPart}";
        }
    }

    // have a standard way to display this stay in the console
    public string DisplayKey
    {
        get
        {
            var place = Place.DisplayName;

            if (!HasDates)
                return place;

            return $"{place} ({StartDate:yyyy-MM-dd}..{EndDate:yyyy-MM-dd})";
        }
    }

    public override string ToString() => DisplayKey;

    public void SetPlace(Place place)
    {
        Place = place ?? throw new ArgumentNullException(nameof(place));
    }

    public void SetPlace(string city, string country)
    {
        Place = new Place(city, country);
    }

    public void SetStatus(StayStatus status)
    {
        Status = status;
    }

    // Expenses
    public Expense AddExpense(string name, decimal amount, ExpenseCategory category, string? notes = null)
    {
        var expense = new Expense(name, amount, category, notes);
        _expenses.Add(expense);
        return expense;
    }

    public void RemoveExpense(Guid expenseId)
    {
        var expense = _expenses.FirstOrDefault(e => e.Id == expenseId);
        if (expense == null)
            throw new InvalidOperationException("Expense not found.");

        _expenses.Remove(expense);
    }

    public Expense GetExpense(Guid expenseId)
    {
        return _expenses.FirstOrDefault(e => e.Id == expenseId)
            ?? throw new InvalidOperationException("Expense not found.");
    }

    // Bookmarks
    public Bookmark AddBookmark(string title, string url, string? notes = null)
    {
        var bookmark = new Bookmark(title, url, notes);
        _bookmarks.Add(bookmark);
        return bookmark;
    }

    public void RemoveBookmark(Guid bookmarkId)
    {
        var bookmark = _bookmarks.FirstOrDefault(b => b.Id == bookmarkId);
        if (bookmark == null)
            throw new InvalidOperationException("Bookmark not found.");

        _bookmarks.Remove(bookmark);
    }

    public Bookmark GetBookmark(Guid bookmarkId)
    {
        return _bookmarks.FirstOrDefault(b => b.Id == bookmarkId)
            ?? throw new InvalidOperationException("Bookmark not found.");
    }

    public FlightOption AddFlightOption(
       string url,
       string fromAirportCode,
       string toAirportCode,
       DateTime departTime,
       DateTime arriveTime,
       decimal? price = null)
    {
        var option = new FlightOption(
            url,
            fromAirportCode,
            toAirportCode,
            departTime,
            arriveTime,
            price);

        _flightOptions.Add(option);
        return option;
    }

    public void RemoveFlightOption(Guid flightOptionId)
    {
        var option = _flightOptions.FirstOrDefault(f => f.Id == flightOptionId);
        if (option == null)
            throw new InvalidOperationException("Flight option not found.");

        _flightOptions.Remove(option);
    }

    public FlightOption GetFlightOption(Guid flightOptionId)
    {
        return _flightOptions.FirstOrDefault(f => f.Id == flightOptionId)
            ?? throw new InvalidOperationException("Flight option not found.");
    }

    public LodgingOption AddLodgingOption(
        string url,
        string propertyName,
        DateTime checkInDate,
        DateTime checkOutDate,
        decimal? price = null)
    {
        var option = new LodgingOption(
            url,
            propertyName,
            checkInDate,
            checkOutDate,
            price);

        _lodgingOptions.Add(option);
        return option;
    }

    public void RemoveLodgingOption(Guid lodgingOptionId)
    {
        var option = _lodgingOptions.FirstOrDefault(l => l.Id == lodgingOptionId);
        if (option == null)
            throw new InvalidOperationException("Lodging option not found.");

        _lodgingOptions.Remove(option);
    }

    public LodgingOption GetLodgingOption(Guid lodgingOptionId)
    {
        return _lodgingOptions.FirstOrDefault(l => l.Id == lodgingOptionId)
            ?? throw new InvalidOperationException("Lodging option not found.");
    }

    public decimal TotalSelectedFlightCost()
    {
        return _flightOptions
            .Where(f => f.IsSelected && f.Price.HasValue)
            .Sum(f => f.Price!.Value);
    }

    public decimal TotalSelectedLodgingCost()
    {
        return _lodgingOptions
            .Where(l => l.IsSelected && l.Price.HasValue)
            .Sum(l => l.Price!.Value);
    }

    public decimal TotalSelectedTravelOptionCost()
    {
        return TotalSelectedFlightCost() + TotalSelectedLodgingCost();
    }

    internal static Stay Hydrate(Guid id, Place place, DateTime? start, DateTime? end, StayStatus status = StayStatus.Idea)
    {
        var stay = new Stay(place);
        stay.Id = id;
        stay.Status = status;

        if (start.HasValue && end.HasValue)
            stay.SetDates(start.Value, end.Value);

        return stay;
    }

    internal void HydrateAddExpense(Expense expense)
    {
        _expenses.Add(expense);
    }

    internal void HydrateAddBookmark(Bookmark bookmark)
    {
        _bookmarks.Add(bookmark);
    }

    internal void HydrateAddFlightOption(FlightOption option)
    {
        _flightOptions.Add(option);
    }

    internal void HydrateAddLodgingOption(LodgingOption option)
    {
        _lodgingOptions.Add(option);
    }
}