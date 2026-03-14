namespace TravelPlanner.Core.Persistence;

public record FlightOptionSnapshot(
    Guid Id,
    string Url,
    DateTime CreatedAt,
    DateTime? LastCheckedAt,
    bool IsSelected,
    string FromAirportCode,
    string ToAirportCode,
    DateTime DepartTime,
    DateTime ArriveTime
);