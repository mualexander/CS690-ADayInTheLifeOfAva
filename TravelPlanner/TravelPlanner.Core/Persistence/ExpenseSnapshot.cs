namespace TravelPlanner.Core.Persistence;

public record ExpenseSnapshot(
    Guid Id,
    DateTime Date,
    decimal Amount,
    string Category,
    string? Note
);