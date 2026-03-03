namespace TravelPlanner.Core.Services;

// DTO returned to UI (Console) so it can list/select locations without touching the full domain object.
public record LocationSummary(Guid Id, string Name, string Country, decimal TotalSpent);