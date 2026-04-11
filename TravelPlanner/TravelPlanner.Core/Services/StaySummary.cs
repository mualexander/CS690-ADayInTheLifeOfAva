using TravelPlanner.Core.Models;

namespace TravelPlanner.Core.Services;

public record StaySummary(
    Guid Id,
    string DisplayKey,
    string City,
    string Country,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal ExpenseTotal,
    decimal SelectedFlightTotal,
    decimal SelectedLodgingTotal,
    decimal TotalPlannedCost,
    StayStatus Status
);