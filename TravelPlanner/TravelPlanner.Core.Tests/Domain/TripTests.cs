using System;
using System.Linq;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class TripTests
{
    [Fact]
    public void Constructor_SetsFieldsCorrectly()
    {
        var trip = new Trip("Japan 2026", 5000m);

        Assert.NotEqual(Guid.Empty, trip.Id);
        Assert.Equal("Japan 2026", trip.Name);
        Assert.Equal(5000m, trip.TotalBudget);
        Assert.False(trip.IsArchived);
        Assert.Empty(trip.Stays);
    }

    [Fact]
    public void Constructor_AllowsZeroBudget()
    {
        var trip = new Trip("Budget Trip", 0m);
        Assert.Equal(0m, trip.TotalBudget);
    }

    [Fact]
    public void Rename_ChangesName()
    {
        var trip = new Trip("Japan", 1000m);
        trip.Rename("Japan Spring 2026");
        Assert.Equal("Japan Spring 2026", trip.Name);
    }

    [Fact]
    public void UpdateBudget_ChangesValue()
    {
        var trip = new Trip("Japan", 1000m);
        trip.UpdateBudget(2000m);
        Assert.Equal(2000m, trip.TotalBudget);
    }

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
        tokyoStay.AddExpense("Meals", 100m, ExpenseCategory.Food);

        var osakaStay = trip.AddStay(new Place("Osaka", "Japan"));
        osakaStay.AddExpense("Meals", 50m, ExpenseCategory.Activities);

        Assert.Equal(150m, trip.TotalPlannedCost());
        Assert.Equal(850m, trip.RemainingBudget());
    }

    [Fact]
    public void RemainingBudget_CanGoNegativeWhenOverspent()
    {
        var trip = new Trip("Japan", 100m);

        var stay = trip.AddStay(new Place("Tokyo", "Japan"));
        stay.AddExpense("Meals", 150m, ExpenseCategory.Food);

        Assert.Equal(150m, trip.TotalPlannedCost());
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

    [Fact]
    public void TotalPlannedCost_IncludesStayExpensesAndSelectedTravelOptions()
    {
        var trip = new Trip("Japan 2026", 5000m);
        var stay = trip.AddStay(new Place("Tokyo", "Japan"));

        stay.AddExpense("Meals", 100m, ExpenseCategory.Food);

        var flight = stay.AddFlightOption(
            "https://example.com/flight", "SFO", "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0),
            500m);
        flight.Select();

        Assert.Equal(600m, trip.TotalPlannedCost());
        Assert.Equal(4400m, trip.RemainingBudget());
    }

    [Fact]
    public void NewTrip_DefaultCurrency_IsUSD()
    {
        var trip = new Trip("Japan 2026", 5000m);
        Assert.Equal("USD", trip.DefaultCurrency);
    }

    [Fact]
    public void NewTrip_HomeAirportCodeAndTravelerCount_AreNull()
    {
        var trip = new Trip("Japan 2026", 5000m);
        Assert.Null(trip.HomeAirportCode);
        Assert.Null(trip.TravelerCount);
    }

    [Fact]
    public void SetHomeAirportCode_NormalizesToUppercase()
    {
        var trip = new Trip("Japan 2026", 5000m);
        trip.SetHomeAirportCode("sfo");
        Assert.Equal("SFO", trip.HomeAirportCode);
    }

    [Fact]
    public void SetHomeAirportCode_ClearsOnBlankOrNull()
    {
        var trip = new Trip("Japan 2026", 5000m);
        trip.SetHomeAirportCode("SFO");
        trip.SetHomeAirportCode("   ");
        Assert.Null(trip.HomeAirportCode);

        trip.SetHomeAirportCode("SFO");
        trip.SetHomeAirportCode(null);
        Assert.Null(trip.HomeAirportCode);
    }

    [Fact]
    public void SetDefaultCurrency_NormalizesToUppercase()
    {
        var trip = new Trip("Japan 2026", 5000m);
        trip.SetDefaultCurrency("jpy");
        Assert.Equal("JPY", trip.DefaultCurrency);
    }

    [Fact]
    public void SetDefaultCurrency_ThrowsOnEmpty()
    {
        var trip = new Trip("Japan 2026", 5000m);
        Assert.Throws<ArgumentException>(() => trip.SetDefaultCurrency(""));
        Assert.Throws<ArgumentException>(() => trip.SetDefaultCurrency("   "));
    }

    [Fact]
    public void SetTravelerCount_StoresValue()
    {
        var trip = new Trip("Japan 2026", 5000m);
        trip.SetTravelerCount(3);
        Assert.Equal(3, trip.TravelerCount);
    }

    [Fact]
    public void SetTravelerCount_ClearsOnNull()
    {
        var trip = new Trip("Japan 2026", 5000m);
        trip.SetTravelerCount(3);
        trip.SetTravelerCount(null);
        Assert.Null(trip.TravelerCount);
    }

    [Fact]
    public void SetTravelerCount_ThrowsOnZeroOrNegative()
    {
        var trip = new Trip("Japan 2026", 5000m);
        Assert.Throws<ArgumentException>(() => trip.SetTravelerCount(0));
        Assert.Throws<ArgumentException>(() => trip.SetTravelerCount(-1));
    }
}