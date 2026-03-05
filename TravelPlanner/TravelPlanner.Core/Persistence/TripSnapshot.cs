namespace TravelPlanner.Core.Persistence;

public record TripSnapshot(
    Guid Id,
    string Name,
    decimal TotalBudget,
    DateTime CreatedAt,
    List<StaySnapshot> Stays
);