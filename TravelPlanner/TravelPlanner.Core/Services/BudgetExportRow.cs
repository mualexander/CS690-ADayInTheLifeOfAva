namespace TravelPlanner.Core.Services;

public record BudgetExportRow(
    string Status,
    string Stay,
    string Type,
    string Name,
    decimal? Amount
);
