using TravelPlanner.Cli.Commands;

namespace TravelPlanner.Cli.Parsers;

public static class StayMenuParser
{
    public static StayMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "v" or "view" => StayMenuCommand.ViewStayDetails,
            "p" or "place" => StayMenuCommand.SetPlace,
            "i" or "checkin" => StayMenuCommand.SetStartDate,
            "o" or "checkout" => StayMenuCommand.SetEndDate,
            "e" or "expenses" => StayMenuCommand.ManageExpenses,
            "b" or "bookmarks" => StayMenuCommand.ManageBookmarks,
            "f" or "flights" => StayMenuCommand.ManageFlightOptions,
            "x" or "delete" => StayMenuCommand.DeleteStay,
            "q" or "back" => StayMenuCommand.Back,
            _ => StayMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input) =>
        (input ?? "").Trim().ToLowerInvariant();
}