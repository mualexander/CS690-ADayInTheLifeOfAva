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

    public Stay(Place place, DateTime? startDate = null, DateTime? endDate = null)
    {
        Place = place ?? throw new ArgumentNullException(nameof(place));
        Id = Guid.NewGuid();

        if (startDate.HasValue || endDate.HasValue)
            SetDates(startDate ?? throw new ArgumentException("Start date required if end date is provided."),
                     endDate ?? throw new ArgumentException("End date required if start date is provided."));
    }

    public void SetDates(DateTime start, DateTime end)
    {
        if (end < start) throw new ArgumentException("End date must be after start date.");
        StartDate = start;
        EndDate = end;
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
        if (e == null) throw new InvalidOperationException("Expense not found.");
        _expenses.Remove(e);
    }

    public decimal TotalSpent() => _expenses.Sum(e => e.Amount);
    public decimal TotalSpent(ExpenseCategory category) => _expenses.Where(e => e.Category == category).Sum(e => e.Amount);
}