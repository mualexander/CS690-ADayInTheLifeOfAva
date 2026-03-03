using System;
using System.Collections.Generic;
using System.Linq;

namespace TravelPlanner.Core.Models;

public class Location
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Country { get; private set; }

    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    private readonly List<Expense> _expenses = new();
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    public Location(string name, string country)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Location name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country cannot be empty.", nameof(country));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Country = country.Trim();
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

    public decimal TotalSpent(ExpenseCategory category) =>
        _expenses.Where(e => e.Category == category).Sum(e => e.Amount);
}