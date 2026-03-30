using Spectre.Console;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

internal static class BudgetWarning
{
    public static void ShowIfOverBudget(TripService svc)
    {
        if (!svc.GetWarnOnOverBudget() || !svc.IsOverBudget()) return;

        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Panel("[yellow bold]This trip is over budget.[/]")
                .BorderColor(Color.Yellow)
                .Header("[yellow] ! Over Budget [/]"));

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[grey]What would you like to do?[/]")
                .AddChoices("Close", "Disable over-budget warnings for this trip"));

        if (choice == "Disable over-budget warnings for this trip")
            svc.SetWarnOnOverBudget(false);
    }
}
