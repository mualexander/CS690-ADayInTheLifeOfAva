using TravelPlanner.Console.Commands;

namespace TravelPlanner.Console.Parsers;

public static class ExpenseMenuParser
{
    public static ExpenseMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "l" or "list" => ExpenseMenuCommand.ListExpenses,
            "a" or "add" => ExpenseMenuCommand.AddExpense,
            "s" or "select" => ExpenseMenuCommand.SelectExpense,
            "x" or "delete" => ExpenseMenuCommand.DeleteExpense,
            "q" or "back" => ExpenseMenuCommand.Back,
            _ => ExpenseMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input)
    {
        return (input ?? "").Trim().ToLowerInvariant();
    }
}