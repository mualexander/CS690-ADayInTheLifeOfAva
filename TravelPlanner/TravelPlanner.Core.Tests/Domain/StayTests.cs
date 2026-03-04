using System;
using System.Linq;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class StayTests
{
    [Fact]
    public void Constructor_RequiresPlace()
    {
        Assert.Throws<ArgumentNullException>(() => new Stay(null!));
    }

    [Fact]
    public void SetDates_EndMustBeAfterStart()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        var start = new DateTime(2026, 1, 10);
        var end = new DateTime(2026, 1, 9);

        Assert.Throws<ArgumentException>(() => stay.SetDates(start, end));
    }

    [Fact]
    public void AddExpense_AddsAndTotalsCorrectly()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));

        stay.AddExpense(DateTime.UtcNow.Date, 10m, ExpenseCategory.Food);
        stay.AddExpense(DateTime.UtcNow.Date, 25m, ExpenseCategory.Food);
        stay.AddExpense(DateTime.UtcNow.Date, 40m, ExpenseCategory.Transportation);

        Assert.Equal(3, stay.Expenses.Count);
        Assert.Equal(75m, stay.TotalSpent());
        Assert.Equal(35m, stay.TotalSpent(ExpenseCategory.Food));
        Assert.Equal(40m, stay.TotalSpent(ExpenseCategory.Transportation));
    }

    [Fact]
    public void RemoveExpense_RemovesExisting()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        var e = stay.AddExpense(DateTime.UtcNow.Date, 10m, ExpenseCategory.Food);

        stay.RemoveExpense(e.Id);

        Assert.Empty(stay.Expenses);
    }

    [Fact]
    public void RemoveExpense_ThrowsWhenMissing()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        Assert.Throws<InvalidOperationException>(() => stay.RemoveExpense(Guid.NewGuid()));
    }
}