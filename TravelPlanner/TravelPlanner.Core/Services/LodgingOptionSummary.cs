namespace TravelPlanner.Core.Services;

public record LodgingOptionSummary(
    Guid Id,
    string Url,
    DateTime CreatedAt,
    DateTime? LastCheckedAt,
    bool IsSelected,
    string PropertyName,
    DateTime CheckInDate,
    DateTime CheckOutDate
);