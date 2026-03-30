using System;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class PlaceTests
{
    [Fact]
    public void Constructor_SetsFieldsCorrectly()
    {
        var place = new Place("Tokyo", "Japan");

        Assert.Equal("Tokyo", place.City);
        Assert.Equal("Japan", place.Country);
        Assert.NotEqual(Guid.Empty, place.Id);
    }

    [Fact]
    public void Constructor_RejectsBlankCity()
    {
        Assert.Throws<ArgumentException>(() => new Place("", "Japan"));
        Assert.Throws<ArgumentException>(() => new Place("   ", "Japan"));
    }

    [Fact]
    public void Constructor_RejectsBlankCountry()
    {
        Assert.Throws<ArgumentException>(() => new Place("Tokyo", ""));
        Assert.Throws<ArgumentException>(() => new Place("Tokyo", "   "));
    }

    [Fact]
    public void DisplayName_IsFormattedCorrectly()
    {
        var place = new Place("Osaka", "Japan");

        Assert.Equal("Osaka, Japan", place.DisplayName);
    }
}
