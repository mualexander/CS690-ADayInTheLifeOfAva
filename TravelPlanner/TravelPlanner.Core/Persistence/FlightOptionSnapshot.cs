namespace TravelPlanner.Core.Persistence;

public record FlightOptionSnapshot(
    Guid Id,
    string Url,
    decimal? Price,
    DateTime CreatedAt,
    DateTime? LastCheckedAt,
    bool IsSelected,
    string FromAirportCode,
    string ToAirportCode,
    DateTime DepartTime,
    DateTime ArriveTime
);