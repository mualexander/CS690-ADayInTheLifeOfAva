using TravelPlanner.Console.Commands;

namespace TravelPlanner.Console.Parsers;

public static class MainMenuParser
{
    public static MainMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "l" or "list" => MainMenuCommand.ListTrips,
            "c" or "create" => MainMenuCommand.CreateTrip,
            "s" or "select" => MainMenuCommand.SelectTrip,
            "t" or "seedtest" => MainMenuCommand.SeedTestData,
            "q" or "quit" => MainMenuCommand.Quit,
            _ => MainMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input)
    {
        return (input ?? "")
            .Trim()
            .ToLowerInvariant();
    }
}