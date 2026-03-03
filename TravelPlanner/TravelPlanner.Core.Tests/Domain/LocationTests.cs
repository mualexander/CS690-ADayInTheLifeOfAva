using System;
using System.Linq;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class LocationTests
{
    [Fact]
    public void Constructor_RequiresNameAndCountry()
    {
        Assert.Throws<ArgumentException>(() => new Location("", "Japan"));
        Assert.Throws<ArgumentException>(() => new Location("Tokyo", ""));
    }

    [Fact]
    public void SetDates_EndMustBeAfterStart()
    {
        var loc = new Location("Tokyo", "Japan");
        var start = new DateTime(2026, 1, 10);
        var end = new DateTime(2026, 1, 9);

        Assert.Throws<ArgumentException>(() => loc.SetDates(start, end));
    }

    [Fact]
    public void AddExpense_AddsAndTotalsCorrectly()
    {
        var loc = new Location("Tokyo", "Japan");
        loc.AddExpense(DateTime.UtcNow.Date, 10m, ExpenseCategory.Food);
        loc.AddExpense(DateTime.UtcNow.Date, 25m, ExpenseCategory.Food);
        loc.AddExpense(DateTime.UtcNow.Date, 40m, ExpenseCategory.Transportation);

        Assert.Equal(3, loc.Expenses.Count);
        Assert.Equal(75m, loc.TotalSpent());
        Assert.Equal(35m, loc.TotalSpent(ExpenseCategory.Food));
        Assert.Equal(40m, loc.TotalSpent(ExpenseCategory.Transportation));
    }

    [Fact]
    public void RemoveExpense_RemovesExisting()
    {
        var loc = new Location("Tokyo", "Japan");
        var e = loc.AddExpense(DateTime.UtcNow.Date, 10m, ExpenseCategory.Food);

        loc.RemoveExpense(e.Id);

        Assert.Empty(loc.Expenses);
    }

    [Fact]
    public void RemoveExpense_ThrowsWhenMissing()
    {
        var loc = new Location("Tokyo", "Japan");
        Assert.Throws<InvalidOperationException>(() => loc.RemoveExpense(Guid.NewGuid()));
    }
}