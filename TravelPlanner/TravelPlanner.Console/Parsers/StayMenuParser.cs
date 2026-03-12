using TravelPlanner.Console.Commands;

namespace TravelPlanner.Console.Parsers;

public static class StayMenuParser
{
    public static StayMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "v" or "view" => StayMenuCommand.ViewStayDetails,
            "a" or "add" => StayMenuCommand.AddExpense,
            "x" or "remove" => StayMenuCommand.RemoveExpense,
            "p" or "place" => StayMenuCommand.SetPlace,
            "i" or "checkin" => StayMenuCommand.SetStartDate,
            "o" or "checkout" => StayMenuCommand.SetEndDate,
            "x" or "delete" => StayMenuCommand.RemoveStay,
            "q" or "back" => StayMenuCommand.Back,
            _ => StayMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input) =>
        (input ?? "").Trim().ToLowerInvariant();
}