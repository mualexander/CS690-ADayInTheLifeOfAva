namespace TravelPlanner.Core.Services;

public record StaySummary(
    Guid Id,
    string City,
    string Country,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal TotalSpent
);