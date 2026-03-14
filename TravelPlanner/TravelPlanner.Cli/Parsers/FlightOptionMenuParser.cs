using TravelPlanner.Cli.Commands;

namespace TravelPlanner.Cli.Parsers;

public static class FlightOptionMenuParser
{
    public static FlightOptionMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "l" or "list" => FlightOptionMenuCommand.ListFlightOptions,
            "a" or "add" => FlightOptionMenuCommand.AddFlightOption,
            "s" or "select" => FlightOptionMenuCommand.SelectFlightOption,
            "d" or "delete" => FlightOptionMenuCommand.DeleteFlightOption,
            "q" or "back" => FlightOptionMenuCommand.Back,
            _ => FlightOptionMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input) =>
        (input ?? "").Trim().ToLowerInvariant();
}