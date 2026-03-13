namespace TravelPlanner.Core.Services;

public record FlightOptionSummary(
    Guid Id,
    string Url,
    DateTime? LastCheckedAt,
    bool IsSelected,
    string FromAirportCode,
    string ToAirportCode,
    DateTime DepartTime,
    DateTime ArriveTime
);