using Spectre.Console;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

public class MainView
{
    private readonly TripService _svc;
    private readonly InMemoryTripContext _ctx;

    public MainView(TripService svc, InMemoryTripContext ctx)
    {
        _svc = svc;
        _ctx = ctx;
    }

    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Render();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Action:[/]")
                    .AddChoices("New Trip", "Open Trip", "Delete Trip", "Seed Demo", "Quit"));

            switch (choice)
            {
                case "New Trip":    OnNew();    break;
                case "Open Trip":   OnOpen();   break;
                case "Delete Trip": OnDelete(); break;
                case "Seed Demo":   OnSeed();   break;
                case "Quit":        return;
            }
        }
    }

    private void Render()
    {
        AnsiConsole.Write(new Rule("[bold deepskyblue1]✈  TravelPlanner[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var trips = _svc.GetTrips();
        if (trips.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey italic]No trips yet. Select \"New Trip\" to create one.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Name[/]")
            .AddColumn(new TableColumn("[bold]Budget[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Cost[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Remaining[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Stays[/]").RightAligned());

        foreach (var t in trips)
        {
            var remainColor = t.RemainingBudget >= 0 ? "green" : "red";
            table.AddRow(
                Markup.Escape(t.Name),
                $"[yellow]${t.TotalBudget:0.00}[/]",
                $"${t.TotalPlannedCost:0.00}",
                $"[{remainColor}]${t.RemainingBudget:0.00}[/]",
                t.StayCount.ToString()
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private void OnNew()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]New Trip[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var name = AnsiConsole.Ask<string>("Trip [bold]name[/]:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var budgetStr = AnsiConsole.Ask<string>("Budget [grey](e.g. 5000)[/]:");
        if (!decimal.TryParse(budgetStr, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var budget) || budget <= 0)
        {
            AnsiConsole.MarkupLine("[red]Invalid budget.[/]");
            Pause();
            return;
        }

        try
        {
            var trip = _svc.CreateTrip(name, budget);
            _svc.SelectTrip(trip.Id);
            new TripView(_svc, _ctx).Run();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
            Pause();
        }
    }

    private void OnOpen()
    {
        var trips = _svc.GetTrips();
        if (trips.Count == 0) { AnsiConsole.MarkupLine("[yellow]No trips to open.[/]"); Pause(); return; }

        var trip = AnsiConsole.Prompt(
            new SelectionPrompt<TripSummary>()
                .Title("Select trip to [bold]open[/]:")
                .UseConverter(t => $"{Markup.Escape(t.Name)}  [grey][[{t.StayCount} stays · ${t.TotalBudget:0}]][/]")
                .AddChoices(trips));

        _svc.SelectTrip(trip.Id);
        new TripView(_svc, _ctx).Run();
    }

    private void OnDelete()
    {
        var trips = _svc.GetTrips();
        if (trips.Count == 0) { AnsiConsole.MarkupLine("[yellow]No trips to delete.[/]"); Pause(); return; }

        var trip = AnsiConsole.Prompt(
            new SelectionPrompt<TripSummary>()
                .Title("Select trip to [bold red]delete[/]:")
                .UseConverter(t => Markup.Escape(t.Name))
                .AddChoices(trips));

        if (!AnsiConsole.Confirm($"Delete [bold]{Markup.Escape(trip.Name)}[/]? This cannot be undone."))
            return;

        try { _svc.DeleteTrip(trip.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnSeed()
    {
        try
        {
            var trip = _svc.CreateTrip("Seed: Japan 2026", 5000m);
            _svc.SelectTrip(trip.Id);
            _svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));
            _svc.AddStay("Osaka", "Japan", new DateTime(2026, 1, 14), new DateTime(2026, 1, 16));
            _svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 16), new DateTime(2026, 1, 20));
            var stays = _svc.GetStays();
            if (stays.Count > 0)
                _svc.AddExpenseToStay(stays[0].Id, "Meals", 180m, ExpenseCategory.Food, "Sushi + ramen");
            AnsiConsole.MarkupLine("[green]Demo data seeded.[/]");
            Pause();
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(intercept: true);
    }
}
