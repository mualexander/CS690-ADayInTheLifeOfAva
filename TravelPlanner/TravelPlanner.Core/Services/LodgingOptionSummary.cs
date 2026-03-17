namespace TravelPlanner.Core.Services;

public record LodgingOptionSummary(
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