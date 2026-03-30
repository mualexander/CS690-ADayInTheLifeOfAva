using Spectre.Console;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

internal static class BudgetWarning
{
    public static void ShowIfOverBudget(TripService svc)
    {
        if (!svc.GetWarnOnOverBudget() || !svc.IsOverBudget()) return;

        var topItems = svc.GetTopCostItems(3);

        var lines = new System.Text.StringBuilder();
        lines.AppendLine("[yellow bold]This trip is over budget.[/]");
        lines.AppendLine();
        lines.AppendLine("[bold]Highest cost items:[/]");
        foreach (var i in topItems)
            lines.AppendLine($"  [grey]{Markup.Escape(i.StayDisplayKey)}[/] \u2013 {Markup.Escape(i.TypeLabel)} \u2013 {Markup.Escape(i.Description)}  [yellow]${i.Price:0.00}[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(new Markup(lines.ToString().TrimEnd())).BorderColor(Color.Yellow).Header("[yellow] ! Over Budget [/]"));

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[grey]What would you like to do?[/]")
                .AddChoices("Close", "Disable over-budget warnings for this trip"));

        if (choice == "Disable over-budget warnings for this trip")
            svc.SetWarnOnOverBudget(false);
    }
}
