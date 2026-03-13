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

    public decimal TotalSpent() => _expenses.Sum(e => e.Amount);

    public decimal TotalSpent(ExpenseCategory category) =>
        _expenses.Where(e => e.Category == category).Sum(e => e.Amount);

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

    internal static Stay Hydrate(Guid id, Place place, DateTime? start, DateTime? end)
    {
        var stay = new Stay(place);
        stay.Id = id;

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
}