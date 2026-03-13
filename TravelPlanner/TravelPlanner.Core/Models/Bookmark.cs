namespace TravelPlanner.Core.Models;

public class Bookmark
{
    public Guid Id { get; internal set; }

    public string Title { get; private set; }

    public string Url { get; private set; }

    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; internal set; }

    public Bookmark(string title, string url, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Bookmark title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Bookmark URL cannot be empty.", nameof(url));

        Id = Guid.NewGuid();
        Title = title.Trim();
        Url = url.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Bookmark title cannot be empty.", nameof(newTitle));

        Title = newTitle.Trim();
    }

    public void UpdateUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("Bookmark URL cannot be empty.", nameof(newUrl));

        Url = newUrl.Trim();
    }

    public void UpdateNotes(string? newNotes)
    {
        Notes = string.IsNullOrWhiteSpace(newNotes) ? null : newNotes.Trim();
    }

    public override string ToString()
    {
        return $"{Title} ({Url})";
    }

    internal static Bookmark Hydrate(Guid id, string title, string url, string? notes, DateTime createdAt)
    {
        var bookmark = new Bookmark(title, url, notes);
        bookmark.Id = id;
        bookmark.CreatedAt = createdAt;
        return bookmark;
    }
}