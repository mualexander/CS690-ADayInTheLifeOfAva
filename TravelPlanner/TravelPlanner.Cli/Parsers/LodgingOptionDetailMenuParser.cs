using TravelPlanner.Cli.Commands;

namespace TravelPlanner.Cli.Parsers;

public static class LodgingOptionDetailMenuParser
{
    public static LodgingOptionDetailMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "v" or "view" => LodgingOptionDetailMenuCommand.ViewDetails,
            "p" or "price" => LodgingOptionDetailMenuCommand.UpdatePrice,
            "u" or "url" => LodgingOptionDetailMenuCommand.UpdateUrl,
            "q" or "back" => LodgingOptionDetailMenuCommand.Back,
            _ => LodgingOptionDetailMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input) =>
        (input ?? "").Trim().ToLowerInvariant();
}