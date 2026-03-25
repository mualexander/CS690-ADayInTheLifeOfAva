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
}