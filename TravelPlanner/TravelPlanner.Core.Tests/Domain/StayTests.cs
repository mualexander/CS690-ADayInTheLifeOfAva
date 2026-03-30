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

        stay.AddExpense("Meals", 10m, ExpenseCategory.Food);
        stay.AddExpense("Snacks", 25m, ExpenseCategory.Food);
        stay.AddExpense("Taxis", 40m, ExpenseCategory.Transportation);

        Assert.Equal(3, stay.Expenses.Count);
        Assert.Equal(75m, stay.TotalExpenses());
        Assert.Equal(35m, stay.TotalExpenses(ExpenseCategory.Food));
        Assert.Equal(40m, stay.TotalExpenses(ExpenseCategory.Transportation));
    }

    [Fact]
    public void RemoveExpense_RemovesExisting()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        var e = stay.AddExpense("Meals", 10m, ExpenseCategory.Food);

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

    [Fact]
    public void SetPlace_UpdatesPlace()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));

        stay.SetPlace(new Place("Osaka", "Japan"));

        Assert.Equal("Osaka", stay.Place.City);
        Assert.Equal("Japan", stay.Place.Country);
    }

    [Fact]
    public void SetStartDate_UpdatesStartDate()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        stay.SetStartDate(new DateTime(2026, 1, 9));

        Assert.Equal(new DateTime(2026, 1, 9), stay.StartDate);
        Assert.Equal(new DateTime(2026, 1, 14), stay.EndDate);
    }

    [Fact]
    public void SetStartDate_ThrowsIfAfterEndDate()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        Assert.Throws<ArgumentException>(() =>
            stay.SetStartDate(new DateTime(2026, 1, 15)));
    }

    [Fact]
    public void SetEndDate_UpdatesEndDate()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        stay.SetEndDate(new DateTime(2026, 1, 15));

        Assert.Equal(new DateTime(2026, 1, 15), stay.EndDate);
    }

    [Fact]
    public void SetEndDate_ThrowsIfBeforeStartDate()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        Assert.Throws<ArgumentException>(() =>
            stay.SetEndDate(new DateTime(2026, 1, 9)));
    }

    [Fact]
    public void DisplayKey_ReflectsUpdatedPlaceAndDates()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        stay.SetPlace(new Place("Osaka", "Japan"));
        stay.SetEndDate(new DateTime(2026, 1, 15));

        Assert.Equal("Osaka, Japan (2026-01-10..2026-01-15)", stay.DisplayKey);
    }

    #region Bookmark Tests

    [Fact]
    public void AddBookmark_AddsBookmark()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));

        var bookmark = stay.AddBookmark("Sushi Place", "https://example.com", "try omakase");

        Assert.Single(stay.Bookmarks);
        Assert.Equal("Sushi Place", bookmark.Title);
        Assert.Equal("https://example.com", bookmark.Url);
        Assert.Equal("try omakase", bookmark.Notes);
    }

    [Fact]
    public void RemoveBookmark_RemovesExistingBookmark()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        var bookmark = stay.AddBookmark("Sushi Place", "https://example.com");

        stay.RemoveBookmark(bookmark.Id);

        Assert.Empty(stay.Bookmarks);
    }

    [Fact]
    public void RemoveBookmark_ThrowsWhenBookmarkNotFound()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));

        Assert.Throws<InvalidOperationException>(() => stay.RemoveBookmark(Guid.NewGuid()));
    }

    [Fact]
    public void GetBookmark_ReturnsMatchingBookmark()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        var bookmark = stay.AddBookmark("Sushi Place", "https://example.com");

        var found = stay.GetBookmark(bookmark.Id);

        Assert.Equal(bookmark.Id, found.Id);
        Assert.Equal("Sushi Place", found.Title);
    }

    #endregion

    #region FlightOption Tests
    [Fact]
    public void AddFlightOption_AddsFlightOption()
    {
        var stay = new Stay(new Place("Santa Cruz", "USA"));

        var option = stay.AddFlightOption(
            "https://example.com/flight",
            "SFO",
            "SJC",
            new DateTime(2026, 3, 10, 8, 0, 0),
            new DateTime(2026, 3, 10, 9, 30, 0));

        Assert.Single(stay.FlightOptions);
        Assert.Equal("SFO", option.FromAirportCode);
        Assert.Equal("SJC", option.ToAirportCode);
    }

    [Fact]
    public void RemoveFlightOption_RemovesExistingFlightOption()
    {
        var stay = new Stay(new Place("Santa Cruz", "USA"));
        var option = stay.AddFlightOption(
            "https://example.com/flight",
            "SFO",
            "SJC",
            new DateTime(2026, 3, 10, 8, 0, 0),
            new DateTime(2026, 3, 10, 9, 30, 0));

        stay.RemoveFlightOption(option.Id);

        Assert.Empty(stay.FlightOptions);
    }

    [Fact]
    public void RemoveFlightOption_ThrowsWhenMissing()
    {
        var stay = new Stay(new Place("Santa Cruz", "USA"));

        Assert.Throws<InvalidOperationException>(() =>
            stay.RemoveFlightOption(Guid.NewGuid()));
    }

    [Fact]
    public void GetFlightOption_ReturnsMatchingFlightOption()
    {
        var stay = new Stay(new Place("Santa Cruz", "USA"));
        var option = stay.AddFlightOption(
            "https://example.com/flight",
            "LAX",
            "SJC",
            new DateTime(2026, 3, 10, 8, 0, 0),
            new DateTime(2026, 3, 10, 9, 30, 0));

        var found = stay.GetFlightOption(option.Id);

        Assert.Equal(option.Id, found.Id);
        Assert.Equal("LAX", found.FromAirportCode);
        Assert.Equal("SJC", found.ToAirportCode);
    }

    [Fact]
    public void TotalSelectedFlightCost_SumsOnlySelectedFlightsWithPrices()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));

        var f1 = stay.AddFlightOption(
            "https://example.com/1", "SFO", "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0),
            500m);

        var f2 = stay.AddFlightOption(
            "https://example.com/2", "LAX", "NRT",
            new DateTime(2026, 1, 10, 9, 0, 0),
            new DateTime(2026, 1, 11, 13, 0, 0),
            600m);

        f1.Select();

        Assert.Equal(500m, stay.TotalSelectedFlightCost());
    }
    #endregion

    #region LodgingOption Tests
    [Fact]
    public void AddLodgingOption_AddsLodgingOption()
    {
        var stay = new Stay(new Place("Kyoto", "Japan"));

        var option = stay.AddLodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        Assert.Single(stay.LodgingOptions);
        Assert.Equal("Budget Inn", option.PropertyName);
        Assert.Equal(new DateTime(2026, 4, 10), option.CheckInDate);
        Assert.Equal(new DateTime(2026, 4, 14), option.CheckOutDate);
    }

    [Fact]
    public void RemoveLodgingOption_RemovesExistingLodgingOption()
    {
        var stay = new Stay(new Place("Kyoto", "Japan"));
        var option = stay.AddLodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        stay.RemoveLodgingOption(option.Id);

        Assert.Empty(stay.LodgingOptions);
    }

    [Fact]
    public void RemoveLodgingOption_ThrowsWhenMissing()
    {
        var stay = new Stay(new Place("Kyoto", "Japan"));

        Assert.Throws<InvalidOperationException>(() =>
            stay.RemoveLodgingOption(Guid.NewGuid()));
    }

    [Fact]
    public void GetLodgingOption_ReturnsMatchingLodgingOption()
    {
        var stay = new Stay(new Place("Kyoto", "Japan"));
        var option = stay.AddLodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        var found = stay.GetLodgingOption(option.Id);

        Assert.Equal(option.Id, found.Id);
        Assert.Equal("Budget Inn", found.PropertyName);
        Assert.Equal(new DateTime(2026, 4, 10), found.CheckInDate);
        Assert.Equal(new DateTime(2026, 4, 14), found.CheckOutDate);
    }

    [Fact]
    public void TotalSelectedLodgingCost_SumsOnlySelectedLodgingWithPrices()
    {
        var stay = new Stay(new Place("Kyoto", "Japan"));

        var l1 = stay.AddLodgingOption(
            "https://example.com/h1", "Budget Inn",
            new DateTime(2026, 4, 10), new DateTime(2026, 4, 12), 300m);

        var l2 = stay.AddLodgingOption(
            "https://example.com/h2", "Fancy Hotel",
            new DateTime(2026, 4, 12), new DateTime(2026, 4, 14), 700m);

        l2.Select();

        Assert.Equal(700m, stay.TotalSelectedLodgingCost());
    }
    #endregion

    [Fact]
    public void HasDates_ReturnsFalse_WhenNoDatesSet()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        Assert.False(stay.HasDates);
        Assert.Null(stay.Days);
        Assert.Null(stay.Nights);
    }

    [Fact]
    public void ClearDates_RemovesDates()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.SetDates(new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));
        Assert.True(stay.HasDates);

        stay.ClearDates();

        Assert.False(stay.HasDates);
        Assert.Null(stay.StartDate);
        Assert.Null(stay.EndDate);
    }

    [Fact]
    public void TotalPlannedCost_IncludesExpensesAndSelectedTravelOptions()
    {
        var stay = new Stay(new Place("Tokyo", "Japan"));
        stay.AddExpense("Meals", 100m, ExpenseCategory.Food);

        var flight = stay.AddFlightOption(
            "https://example.com/flight", "SFO", "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0),
            500m);
        flight.Select();

        var lodging = stay.AddLodgingOption(
            "https://example.com/hotel", "Hotel",
            new DateTime(2026, 1, 11), new DateTime(2026, 1, 14),
            400m);
        lodging.Select();

        Assert.Equal(1000m, stay.TotalPlannedCost());
    }
}