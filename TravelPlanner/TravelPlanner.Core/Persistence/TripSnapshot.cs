namespace TravelPlanner.Core.Persistence;

public record TripSnapshot(
    Guid Id,
    string Name,
    decimal TotalBudget,
    DateTime CreatedAt,
    List<StaySnapshot> Stays
)
{
    // Nullable for backward compatibility: old JSON without this field deserializes to null.
    public bool? WarnOnOverBudget { get; init; }
    public string? HomeAirportCode { get; init; }
    public string? DefaultCurrency { get; init; }
    public int? TravelerCount { get; init; }
};