namespace TravelPlanner.Core.Models;

public abstract class TravelOption
{
    public Guid Id { get; internal set; }

    public string Url { get; private set; }

    public decimal? Price { get; private set; }

    public DateTime CreatedAt { get; internal set; }

    public DateTime? LastCheckedAt { get; private set; }

    public bool IsSelected { get; private set; }

    protected TravelOption(string url, decimal? price = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Travel option URL cannot be empty.", nameof(url));

        if (price.HasValue && price.Value < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        Id = Guid.NewGuid();
        Url = url.Trim();
        Price = price;
        CreatedAt = DateTime.UtcNow;

        if (price.HasValue)
            LastCheckedAt = DateTime.UtcNow;
    }


    public void UpdateUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("Travel option URL cannot be empty.", nameof(newUrl));

        Url = newUrl.Trim();
    }

    public void UpdatePrice(decimal? newPrice)
    {
        if (newPrice.HasValue && newPrice.Value < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));

        Price = newPrice;

        if (newPrice.HasValue)
            LastCheckedAt = DateTime.UtcNow;
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