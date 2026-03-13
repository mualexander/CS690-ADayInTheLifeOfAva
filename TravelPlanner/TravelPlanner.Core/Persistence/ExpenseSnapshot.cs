namespace TravelPlanner.Core.Persistence;

public record ExpenseSnapshot(
    Guid Id,
    string Name,
    decimal Amount,
    string Category,
    string? Note,
    DateTime CreatedAt
);