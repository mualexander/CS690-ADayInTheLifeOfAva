namespace TravelPlanner.Core.Services;

// DTO returned to UI so it can list/select trips without coupling to the domain model.
public record TripSummary(
    Guid Id,
    string Name,
    decimal TotalBudget,
    decimal TotalSpent,
    decimal RemainingBudget,
    int LocationCount
);