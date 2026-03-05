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

    [Fact]
    public void Days_IsInclusive_WhenStartEqualsEnd_IsOneDay()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 10));

        Assert.True(stay.HasDates);
        Assert.Equal(1, stay.Days);
        Assert.Equal(0, stay.Nights);
    }

    [Fact]
    public void Days_AndNights_AreComputedCorrectly_ForMultiDayStay()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        // Inclusive days: 10,11,12,13,14 => 5 days
        Assert.Equal(5, stay.Days);

        // Nights: days - 1 => 4 nights
        Assert.Equal(4, stay.Nights);
    }

    [Fact]
    public void SetDates_StripsTimeComponents()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(
            new DateTime(2026, 1, 10, 23, 59, 0),
            new DateTime(2026, 1, 11, 0, 1, 0)
        );

        Assert.Equal(new DateTime(2026, 1, 10), stay.StartDate);
        Assert.Equal(new DateTime(2026, 1, 11), stay.EndDate);
        Assert.Equal(2, stay.Days);   // inclusive
        Assert.Equal(1, stay.Nights); // days-1
    }

    [Fact]
    public void DisplayKey_WithDates_IsFormattedCorrectly()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        Assert.Equal(
            "Tokyo, Japan (2026-01-10..2026-01-14)",
            stay.DisplayKey
        );
    }

    [Fact]
    public void DisplayKey_WithoutDates_IsPlaceName()
    {
        var stay = new Stay(new Place("Osaka", "Japan"));

        Assert.Equal("Osaka, Japan", stay.DisplayKey);
    }
}