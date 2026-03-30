namespace TravelPlanner.Core.Services;

public record CostItemSummary(
    string StayDisplayKey,
    string TypeLabel,
    string Description,
    decimal Price
);
