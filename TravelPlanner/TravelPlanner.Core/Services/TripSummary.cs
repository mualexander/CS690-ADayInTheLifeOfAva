namespace TravelPlanner.Core.Services;

public record TripSummary(
    Guid Id,
    string Name,
    decimal TotalBudget,
    decimal TotalSpent,
    decimal RemainingBudget,
    int StayCount
);