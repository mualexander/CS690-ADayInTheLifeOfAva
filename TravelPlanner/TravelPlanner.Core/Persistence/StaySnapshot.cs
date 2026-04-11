namespace TravelPlanner.Core.Persistence;

public record StaySnapshot(
    Guid Id,
    string City,
    string Country,
    DateTime? StartDate,
    DateTime? EndDate,
    List<ExpenseSnapshot> Expenses,
    List<BookmarkSnapshot> Bookmarks,
    List<FlightOptionSnapshot> FlightOptions,
    List<LodgingOptionSnapshot> LodgingOptions
)
{
    public string? Status { get; init; }
};