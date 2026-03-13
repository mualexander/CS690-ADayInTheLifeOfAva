using System;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class BookmarkTests
{
    [Fact]
    public void Constructor_SetsFieldsCorrectly()
    {
        var bookmark = new Bookmark("Sushi Place", "https://example.com", "try omakase");

        Assert.Equal("Sushi Place", bookmark.Title);
        Assert.Equal("https://example.com", bookmark.Url);
        Assert.Equal("try omakase", bookmark.Notes);
        Assert.NotEqual(Guid.Empty, bookmark.Id);
    }

    [Fact]
    public void Constructor_RejectsBlankTitle()
    {
        Assert.Throws<ArgumentException>(() =>
            new Bookmark("", "https://example.com"));

        Assert.Throws<ArgumentException>(() =>
            new Bookmark("   ", "https://example.com"));
    }

    [Fact]
    public void Constructor_RejectsBlankUrl()
    {
        Assert.Throws<ArgumentException>(() =>
            new Bookmark("Sushi Place", ""));

        Assert.Throws<ArgumentException>(() =>
            new Bookmark("Sushi Place", "   "));
    }

    [Fact]
    public void Rename_ChangesTitle()
    {
        var bookmark = new Bookmark("Old Title", "https://example.com");

        bookmark.Rename("New Title");

        Assert.Equal("New Title", bookmark.Title);
    }

    [Fact]
    public void Rename_RejectsBlankTitle()
    {
        var bookmark = new Bookmark("Old Title", "https://example.com");

        Assert.Throws<ArgumentException>(() => bookmark.Rename(""));
        Assert.Throws<ArgumentException>(() => bookmark.Rename("   "));
    }

    [Fact]
    public void UpdateUrl_ChangesUrl()
    {
        var bookmark = new Bookmark("Sushi Place", "https://old.example");

        bookmark.UpdateUrl("https://new.example");

        Assert.Equal("https://new.example", bookmark.Url);
    }

    [Fact]
    public void UpdateUrl_RejectsBlankUrl()
    {
        var bookmark = new Bookmark("Sushi Place", "https://example.com");

        Assert.Throws<ArgumentException>(() => bookmark.UpdateUrl(""));
        Assert.Throws<ArgumentException>(() => bookmark.UpdateUrl("   "));
    }

    [Fact]
    public void UpdateNotes_ChangesNotes()
    {
        var bookmark = new Bookmark("Sushi Place", "https://example.com", "old notes");

        bookmark.UpdateNotes("new notes");

        Assert.Equal("new notes", bookmark.Notes);
    }

    [Fact]
    public void UpdateNotes_ClearsNotesWhenBlank()
    {
        var bookmark = new Bookmark("Sushi Place", "https://example.com", "old notes");

        bookmark.UpdateNotes("");
        Assert.Null(bookmark.Notes);

        bookmark.UpdateNotes("   ");
        Assert.Null(bookmark.Notes);
    }
}