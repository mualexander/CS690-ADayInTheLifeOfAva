using TravelPlanner.Console.Commands;

namespace TravelPlanner.Console.Parsers;

public static class TripMenuParser
{
    public static TripMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "v" or "view" => TripMenuCommand.ViewTripSummary,
            "l" or "list" => TripMenuCommand.ListStays,
            "a" or "add" => TripMenuCommand.AddStay,
            "s" or "select" => TripMenuCommand.SelectStay,
            "r" or "rename" => TripMenuCommand.RenameTrip,
            "b" or "budget" => TripMenuCommand.UpdateBudget,
            "x" or "archive" => TripMenuCommand.ArchiveTrip,
            "q" or "back" => TripMenuCommand.Back,
            _ => TripMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input) =>
        (input ?? "").Trim().ToLowerInvariant();
}