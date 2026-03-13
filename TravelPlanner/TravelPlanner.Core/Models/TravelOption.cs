namespace TravelPlanner.Core.Models;

public abstract class TravelOption
{
    public Guid Id { get; internal set; }

    public string Url { get; private set; }

    public DateTime CreatedAt { get; internal set; }

    public DateTime? LastCheckedAt { get; private set; }

    public bool IsSelected { get; private set; }

    protected TravelOption(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Travel option URL cannot be empty.", nameof(url));

        Id = Guid.NewGuid();
        Url = url.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("Travel option URL cannot be empty.", nameof(newUrl));

        Url = newUrl.Trim();
    }

    public void MarkChecked(DateTime? checkedAt = null)
    {
        LastCheckedAt = checkedAt ?? DateTime.UtcNow;
    }

    public void Select()
    {
        IsSelected = true;
    }

    public void Deselect()
    {
        IsSelected = false;
    }
}