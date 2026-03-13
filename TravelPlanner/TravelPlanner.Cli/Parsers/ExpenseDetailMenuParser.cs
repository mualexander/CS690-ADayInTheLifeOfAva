namespace TravelPlanner.Cli;

public static class ExpenseDetailMenuParser
{
    public static ExpenseDetailMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "v" or "view" => ExpenseDetailMenuCommand.ViewDetails,
            "r" or "rename" => ExpenseDetailMenuCommand.Rename,
            "u" or "amount" => ExpenseDetailMenuCommand.UpdateAmount,
            "n" or "notes" => ExpenseDetailMenuCommand.UpdateNotes,
            "x" or "delete" => ExpenseDetailMenuCommand.Delete,
            "q" or "back" => ExpenseDetailMenuCommand.Back,
            _ => ExpenseDetailMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input)
    {
        return (input ?? "").Trim().ToLowerInvariant();
    }
}