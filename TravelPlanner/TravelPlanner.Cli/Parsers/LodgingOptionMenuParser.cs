using TravelPlanner.Cli.Commands;

namespace TravelPlanner.Cli.Parsers;

public static class LodgingOptionMenuParser
{
    public static LodgingOptionMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "l" or "list" => LodgingOptionMenuCommand.ListLodgingOptions,
            "a" or "add" => LodgingOptionMenuCommand.AddLodgingOption,
            "s" or "select" => LodgingOptionMenuCommand.SelectLodgingOption,
            "d" or "delete" => LodgingOptionMenuCommand.DeleteLodgingOption,
            "q" or "back" => LodgingOptionMenuCommand.Back,
            _ => LodgingOptionMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input) =>
        (input ?? "").Trim().ToLowerInvariant();
}