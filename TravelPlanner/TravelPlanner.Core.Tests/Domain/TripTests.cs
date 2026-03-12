using System;
using System.Linq;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class TripTests
{
    [Fact]
    public void Constructor_RejectsBlankName()
    {
        Assert.Throws<ArgumentException>(() => new Trip("", 1000m));
        Assert.Throws<ArgumentException>(() => new Trip("   ", 1000m));
    }

    [Fact]
    public void Constructor_RejectsNegativeBudget()
    {
        Assert.Throws<ArgumentException>(() => new Trip("Japan", -1m));
    }

    [Fact]
    public void Rename_RejectsBlank()
    {
        var trip = new Trip("Japan", 1000m);
        Assert.Throws<ArgumentException>(() => trip.Rename(""));
        Assert.Throws<ArgumentException>(() => trip.Rename("   "));
    }

    [Fact]
    public void UpdateBudget_RejectsNegative()
    {
        var trip = new Trip("Japan", 1000m);
        Assert.Throws<ArgumentException>(() => trip.UpdateBudget(-1m));
    }

    [Fact]
    public void AddStay_AllowsMultipleStaysInSamePlace()
    {
        var trip = new Trip("Japan", 1000m);
        var tokyo = new Place("Tokyo", "Japan");

        trip.AddStay(tokyo, new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));
        trip.AddStay(tokyo, new DateTime(2026, 1, 16), new DateTime(2026, 1, 20));

        Assert.Equal(2, trip.Stays.Count);
        Assert.All(trip.Stays, s => Assert.Equal("Tokyo", s.Place.City));
    }

    [Fact]
    public void RemoveStay_ThrowsWhenMissing()
    {
        var trip = new Trip("Japan", 1000m);
        Assert.Throws<InvalidOperationException>(() => trip.RemoveStay(Guid.NewGuid()));
    }

    [Fact]
    public void TripTotals_RollUpAcrossStays()
    {
        var trip = new Trip("Japan", 1000m);

        var tokyoStay = trip.AddStay(new Place("Tokyo", "Japan"));
        tokyoStay.AddExpense(DateTime.UtcNow.Date, 100m, ExpenseCategory.Food);

        var osakaStay = trip.AddStay(new Place("Osaka", "Japan"));
        osakaStay.AddExpense(DateTime.UtcNow.Date, 50m, ExpenseCategory.Activities);

        Assert.Equal(150m, trip.TotalSpent());
        Assert.Equal(850m, trip.RemainingBudget());
    }

    [Fact]
    public void RemainingBudget_CanGoNegativeWhenOverspent()
    {
        var trip = new Trip("Japan", 100m);

        var stay = trip.AddStay(new Place("Tokyo", "Japan"));
        stay.AddExpense(DateTime.UtcNow.Date, 150m, ExpenseCategory.Food);

        Assert.Equal(150m, trip.TotalSpent());
        Assert.Equal(-50m, trip.RemainingBudget());
    }

    [Fact]
    public void Archive_SetsIsArchivedToTrue()
    {
        var trip = new Trip("Japan 2026", 5000m);

        trip.Archive();

        Assert.True(trip.IsArchived);
    }

    [Fact]
    public void Archive_CanBeCalledMoreThanOnce_AndRemainsArchived()
    {
        var trip = new Trip("Japan 2026", 5000m);

        trip.Archive();
        trip.Archive();

        Assert.True(trip.IsArchived);
    }

    [Fact]
    public void Rename_ThrowsWhenTripIsArchived()
    {
        var trip = new Trip("Japan 2026", 5000m);
        trip.Archive();

        Assert.Throws<InvalidOperationException>(() => trip.Rename("Japan Spring 2026"));
    }

    [Fact]
    public void UpdateBudget_ThrowsWhenTripIsArchived()
    {
        var trip = new Trip("Japan 2026", 5000m);
        trip.Archive();

        Assert.Throws<InvalidOperationException>(() => trip.UpdateBudget(6000m));
    }

    [Fact]
    public void AddStay_ThrowsWhenTripIsArchived()
    {
        var trip = new Trip("Japan 2026", 5000m);
        trip.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            trip.AddStay(new Place("Tokyo", "Japan")));
    }
}