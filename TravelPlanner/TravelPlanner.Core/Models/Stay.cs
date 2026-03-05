using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelPlanner.Core.Models;

public class Stay
{
    public Guid Id { get; private set; }
    public Place Place { get; private set; }

    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    private readonly List<Expense> _expenses = new();
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

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

    public Expense AddExpense(DateTime date, decimal amount, ExpenseCategory category, string? note = null)
    {
        var expense = new Expense(date, amount, category, note);
        _expenses.Add(expense);
        return expense;
    }

    public void RemoveExpense(Guid expenseId)
    {
        var e = _expenses.FirstOrDefault(x => x.Id == expenseId);
        if (e == null)
            throw new InvalidOperationException("Expense not found.");

        _expenses.Remove(e);
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

}