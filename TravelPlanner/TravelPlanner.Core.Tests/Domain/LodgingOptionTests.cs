using System;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class LodgingOptionTests
{
    [Fact]
    public void Constructor_SetsFieldsCorrectly()
    {
        var option = new LodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        Assert.Equal("https://example.com/hotel", option.Url);
        Assert.Equal("Budget Inn", option.PropertyName);
        Assert.Equal(new DateTime(2026, 4, 10), option.CheckInDate);
        Assert.Equal(new DateTime(2026, 4, 14), option.CheckOutDate);
        Assert.False(option.IsSelected);
        Assert.Null(option.LastCheckedAt);
        Assert.NotEqual(Guid.Empty, option.Id);
        Assert.NotEqual(default, option.CreatedAt);
    }

    [Fact]
    public void Constructor_RejectsBlankPropertyName()
    {
        Assert.Throws<ArgumentException>(() =>
            new LodgingOption(
                "https://example.com/hotel",
                "",
                new DateTime(2026, 4, 10),
                new DateTime(2026, 4, 14)));

        Assert.Throws<ArgumentException>(() =>
            new LodgingOption(
                "https://example.com/hotel",
                "   ",
                new DateTime(2026, 4, 10),
                new DateTime(2026, 4, 14)));
    }

    [Fact]
    public void Constructor_RejectsCheckoutBeforeCheckin()
    {
        Assert.Throws<ArgumentException>(() =>
            new LodgingOption(
                "https://example.com/hotel",
                "Budget Inn",
                new DateTime(2026, 4, 14),
                new DateTime(2026, 4, 10)));
    }

    [Fact]
    public void RenameProperty_ChangesName()
    {
        var option = new LodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        option.RenameProperty("Fancy Hotel");

        Assert.Equal("Fancy Hotel", option.PropertyName);
    }

    [Fact]
    public void RenameProperty_RejectsBlankName()
    {
        var option = new LodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        Assert.Throws<ArgumentException>(() => option.RenameProperty(""));
        Assert.Throws<ArgumentException>(() => option.RenameProperty("   "));
    }

    [Fact]
    public void UpdateDates_ChangesDates()
    {
        var option = new LodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        option.UpdateDates(
            new DateTime(2026, 4, 11),
            new DateTime(2026, 4, 15));

        Assert.Equal(new DateTime(2026, 4, 11), option.CheckInDate);
        Assert.Equal(new DateTime(2026, 4, 15), option.CheckOutDate);
    }

    [Fact]
    public void UpdateDates_RejectsCheckoutBeforeCheckin()
    {
        var option = new LodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        Assert.Throws<ArgumentException>(() =>
            option.UpdateDates(
                new DateTime(2026, 4, 15),
                new DateTime(2026, 4, 11)));
    }

    [Fact]
    public void UpdateUrl_ChangesUrl()
    {
        var option = new LodgingOption(
            "https://old.example/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        option.UpdateUrl("https://new.example/hotel");

        Assert.Equal("https://new.example/hotel", option.Url);
    }

    [Fact]
    public void MarkChecked_SetsLastCheckedAt()
    {
        var option = new LodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        var checkedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        option.MarkChecked(checkedAt);

        Assert.Equal(checkedAt, option.LastCheckedAt);
    }

    [Fact]
    public void Select_And_Deselect_UpdateSelectionState()
    {
        var option = new LodgingOption(
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        option.Select();
        Assert.True(option.IsSelected);

        option.Deselect();
        Assert.False(option.IsSelected);
    }
}