using System.Collections.Generic;

namespace TravelPlanner.Core.Services;

public record TagSearchResultSummary(
    string TripName,
    string StayDisplayKey,
    string Title,
    string Url,
    string? Notes,
    IReadOnlyCollection<string> Tags
);
