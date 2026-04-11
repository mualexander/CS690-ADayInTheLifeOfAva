namespace TravelPlanner.Core.Persistence;

public record BookmarkSnapshot(
    Guid Id,
    string Title,
    string Url,
    string? Notes,
    DateTime CreatedAt
)
{
    public List<string>? Tags { get; init; }
};