namespace TravelPlanner.Core.Persistence;

public record LodgingOptionSnapshot(
    Guid Id,
    string Url,
    DateTime CreatedAt,
    DateTime? LastCheckedAt,
    bool IsSelected,
    string PropertyName,
    DateTime CheckInDate,
    DateTime CheckOutDate
);