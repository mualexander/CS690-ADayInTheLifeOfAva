using System;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class BookmarkTests
{
    [Fact]
    public void Constructor_SetsFieldsCorrectly()
    {
        var b = new Bookmark("Sushi Place", "https://example.com", "try omakase");

        Assert.Equal("Sushi Place", b.Title);
        Assert.Equal("https://example.com", b.Url);
        Assert.Equal("try omakase", b.Notes);
    }

    [Fact]
    public void Rename_ChangesTitle()
    {
        var b = new Bookmark("Old", "https://example.com");

        b.Rename("New");

        Assert.Equal("New", b.Title);
    }

}
