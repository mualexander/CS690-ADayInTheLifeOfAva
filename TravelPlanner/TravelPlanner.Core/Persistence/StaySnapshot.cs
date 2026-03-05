namespace TravelPlanner.Core.Persistence;

public record StaySnapshot(
    Guid Id,
    string City,
    string Country,
    DateTime? StartDate,
    DateTime? EndDate,
    List<ExpenseSnapshot> Expenses
);