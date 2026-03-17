namespace TravelPlanner.Core.Models;

public class LodgingOption : TravelOption
{
    public string PropertyName { get; private set; }
    public DateTime CheckInDate { get; private set; }
    public DateTime CheckOutDate { get; private set; }

    public LodgingOption(
        string url,
        string propertyName,
        DateTime checkInDate,
        DateTime checkOutDate)
        : base(url)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(propertyName));

        checkInDate = checkInDate.Date;
        checkOutDate = checkOutDate.Date;

        if (checkOutDate < checkInDate)
            throw new ArgumentException("Check-out date cannot be before check-in date.");

        PropertyName = propertyName.Trim();
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    public void RenameProperty(string newPropertyName)
    {
        if (string.IsNullOrWhiteSpace(newPropertyName))
            throw new ArgumentException("Property name cannot be empty.", nameof(newPropertyName));

        PropertyName = newPropertyName.Trim();
    }

    public void UpdateDates(DateTime checkInDate, DateTime checkOutDate)
    {
        checkInDate = checkInDate.Date;
        checkOutDate = checkOutDate.Date;

        if (checkOutDate < checkInDate)
            throw new ArgumentException("Check-out date cannot be before check-in date.");

        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    internal static LodgingOption Hydrate(
        Guid id,
        string url,
        DateTime createdAt,
        DateTime? lastCheckedAt,
        bool isSelected,
        string propertyName,
        DateTime checkInDate,
        DateTime checkOutDate)
    {
        var option = new LodgingOption(url, propertyName, checkInDate, checkOutDate)
        {
            Id = id,
            CreatedAt = createdAt
        };

        if (lastCheckedAt.HasValue)
            option.MarkChecked(lastCheckedAt.Value);

        if (isSelected)
            option.Select();

        return option;
    }
}