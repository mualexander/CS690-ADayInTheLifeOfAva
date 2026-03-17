namespace TravelPlanner.Core.Services;

public record TripSummary(
    Guid Id,
    string Name,
    decimal TotalBudget,
    decimal ExpenseTotal,
    decimal SelectedTravelOptionTotal,
    decimal TotalPlannedCost,
    decimal RemainingBudget,
    int StayCount
);