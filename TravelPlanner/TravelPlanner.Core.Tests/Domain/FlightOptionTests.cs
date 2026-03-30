using System;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class FlightOptionTests
{
    [Fact]
    public void Constructor_SetsFieldsCorrectly()
    {
        var flight = new FlightOption(
            "https://example.com/flight",
            "sfo",
            "hnd",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0)
        );

        Assert.Equal("https://example.com/flight", flight.Url);
        Assert.Equal("SFO", flight.FromAirportCode);
        Assert.Equal("HND", flight.ToAirportCode);
        Assert.False(flight.IsSelected);
        Assert.Null(flight.LastCheckedAt);
    }

    [Fact]
    public void UpdateUrl_ChangesUrl()
    {
        var flight = new FlightOption(
            "https://old.example",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0)
        );

        flight.UpdateUrl("https://new.example");

        Assert.Equal("https://new.example", flight.Url);
    }

    [Fact]
    public void MarkChecked_SetsLastCheckedAt()
    {
        var flight = new FlightOption(
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0)
        );

        var checkedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        flight.MarkChecked(checkedAt);

        Assert.Equal(checkedAt, flight.LastCheckedAt);
    }

    [Fact]
    public void Select_And_Deselect_UpdateSelectionState()
    {
        var flight = new FlightOption(
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0)
        );

        flight.Select();
        Assert.True(flight.IsSelected);

        flight.Deselect();
        Assert.False(flight.IsSelected);
    }

    [Fact]
    public void UpdateRoute_NormalizesAirportCodes()
    {
        var flight = new FlightOption(
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0)
        );

        flight.UpdateRoute(" lax ", " nrt ");

        Assert.Equal("LAX", flight.FromAirportCode);
        Assert.Equal("NRT", flight.ToAirportCode);
    }

    [Fact]
    public void Constructor_SetsCreatedAt()
    {
        var flight = new FlightOption(
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0)
        );

        Assert.True(flight.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithPrice_SetsPriceAndLastCheckedAt()
    {
        var option = new FlightOption(
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0),
            499.99m);

        Assert.Equal(499.99m, option.Price);
        Assert.NotNull(option.LastCheckedAt);
    }

    [Fact]
    public void UpdatePrice_ChangesPriceAndSetsLastCheckedAt()
    {
        var option = new FlightOption(
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0));

        Assert.Null(option.LastCheckedAt);

        option.UpdatePrice(550m);

        Assert.Equal(550m, option.Price);
        Assert.NotNull(option.LastCheckedAt);
    }

    [Fact]
    public void Constructor_RejectsBlankUrl()
    {
        Assert.Throws<ArgumentException>(() =>
            new FlightOption("", "SFO", "HND",
                new DateTime(2026, 1, 10, 8, 0, 0),
                new DateTime(2026, 1, 11, 12, 0, 0)));
    }

    [Fact]
    public void Constructor_RejectsBlankAirportCodes()
    {
        Assert.Throws<ArgumentException>(() =>
            new FlightOption("https://example.com", "", "HND",
                new DateTime(2026, 1, 10, 8, 0, 0),
                new DateTime(2026, 1, 11, 12, 0, 0)));

        Assert.Throws<ArgumentException>(() =>
            new FlightOption("https://example.com", "SFO", "",
                new DateTime(2026, 1, 10, 8, 0, 0),
                new DateTime(2026, 1, 11, 12, 0, 0)));
    }

    [Fact]
    public void Constructor_RejectsNegativePrice()
    {
        Assert.Throws<ArgumentException>(() =>
            new FlightOption("https://example.com", "SFO", "HND",
                new DateTime(2026, 1, 10, 8, 0, 0),
                new DateTime(2026, 1, 11, 12, 0, 0),
                price: -1m));
    }

    [Fact]
    public void UpdateUrl_RejectsBlankUrl()
    {
        var flight = new FlightOption("https://example.com", "SFO", "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0));

        Assert.Throws<ArgumentException>(() => flight.UpdateUrl(""));
        Assert.Throws<ArgumentException>(() => flight.UpdateUrl("   "));
    }

    [Fact]
    public void UpdateRoute_RejectsBlankAirportCodes()
    {
        var flight = new FlightOption("https://example.com", "SFO", "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0));

        Assert.Throws<ArgumentException>(() => flight.UpdateRoute("", "NRT"));
        Assert.Throws<ArgumentException>(() => flight.UpdateRoute("LAX", ""));
    }

    [Fact]
    public void UpdateTimes_ChangesDepartAndArriveTime()
    {
        var flight = new FlightOption("https://example.com", "SFO", "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0));

        var newDepart = new DateTime(2026, 2, 1, 9, 0, 0);
        var newArrive = new DateTime(2026, 2, 2, 14, 0, 0);
        flight.UpdateTimes(newDepart, newArrive);

        Assert.Equal(newDepart, flight.DepartTime);
        Assert.Equal(newArrive, flight.ArriveTime);
    }

    [Fact]
    public void ArrivalBeforeDeparture_IsAllowed_ForDatelineCrossing()
    {
        // Auckland (NZT) to Los Angeles (PT): local arrival is "before" local departure
        // e.g. depart AKL 2026-01-10 09:00, arrive LAX 2026-01-09 19:00 in local times
        var depart = new DateTime(2026, 1, 10, 9, 0, 0);
        var arrive = new DateTime(2026, 1, 9, 19, 0, 0);

        var flight = new FlightOption("https://example.com", "AKL", "LAX", depart, arrive);

        Assert.Equal(depart, flight.DepartTime);
        Assert.Equal(arrive, flight.ArriveTime);
    }
}