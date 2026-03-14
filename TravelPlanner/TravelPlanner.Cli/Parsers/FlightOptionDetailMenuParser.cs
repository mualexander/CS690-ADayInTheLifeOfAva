using TravelPlanner.Cli.Commands;

namespace TravelPlanner.Cli.Parsers;

public static class FlightOptionDetailMenuParser
{
    public static FlightOptionDetailMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "v" or "view" => FlightOptionDetailMenuCommand.ViewDetails,
            "q" or "back" => FlightOptionDetailMenuCommand.Back,
            _ => FlightOptionDetailMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input) =>
        (input ?? "").Trim().ToLowerInvariant();
}