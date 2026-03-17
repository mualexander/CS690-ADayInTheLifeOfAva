namespace TravelPlanner.Core.Persistence;

public record LodgingOptionSnapshot(
    Guid Id,
    string Url,
    decimal? Price,
    DateTime CreatedAt,
    DateTime? LastCheckedAt,
    bool IsSelected,
    string PropertyName,
    DateTime CheckInDate,
    DateTime CheckOutDate
);