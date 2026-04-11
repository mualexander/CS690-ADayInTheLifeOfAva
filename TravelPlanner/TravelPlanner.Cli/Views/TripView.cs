using Spectre.Console;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

public class TripView
{
    private readonly TripService _svc;
    private readonly InMemoryTripContext _ctx;

    public TripView(TripService svc, InMemoryTripContext ctx)
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
                    .AddChoices("Add Stay", "Open Stay", "Set Status", "Delete Stay", "Update Budget", "Toggle Over-Budget Warning", "Archive Trip", "Back"));

            try
            {
                switch (choice)
                {
                    case "Add Stay":                   OnAddStay();                      break;
                    case "Open Stay":                  OnOpenStay();                     break;
                    case "Set Status":                 OnSetStatus();                    break;
                    case "Delete Stay":                OnDeleteStay();                   break;
                    case "Update Budget":              OnUpdateBudget();                 break;
                    case "Toggle Over-Budget Warning": OnToggleWarnOnOverBudget();       break;
                    case "Archive Trip":               if (OnArchive()) return;          break;
                    case "Back":                       return;
                }
            }
            catch (OperationCanceledException) { }
        }
    }

    private void Render()
    {
        var trip = _ctx.ActiveTrip;
        var title = trip is not null ? Markup.Escape(trip.Name) : "Trip";
        AnsiConsole.Write(new Rule($"[bold deepskyblue1]{title}[/]").RuleStyle("deepskyblue1"));

        if (trip is not null)
        {
            var remainColor = trip.RemainingBudget() >= 0 ? "green" : "red";
            var warnStatus = trip.TotalBudget > 0
                ? (trip.WarnOnOverBudget ? "  [grey]Warnings: On[/]" : "  [grey]Warnings: Off[/]")
                : "";
            AnsiConsole.MarkupLine(
                $"  Budget: [yellow]${trip.TotalBudget:0.00}[/]   " +
                $"Cost: ${trip.TotalPlannedCost():0.00}   " +
                $"Remaining: [{remainColor}]${trip.RemainingBudget():0.00}[/]" +
                warnStatus);
        }

        AnsiConsole.WriteLine();

        var stays = _svc.GetStays();
        if (stays.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey italic]No stays yet.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Stay[/]")
            .AddColumn("[bold]Status[/]")
            .AddColumn("[bold]Dates[/]")
            .AddColumn(new TableColumn("[bold]Expenses[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Flights[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Lodging[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Total[/]").RightAligned());

        foreach (var s in stays)
        {
            var dates = s.StartDate.HasValue
                ? $"{s.StartDate:yyyy-MM-dd} → {s.EndDate:yyyy-MM-dd}"
                : "[grey](no dates)[/]";
            table.AddRow(
                Markup.Escape(s.DisplayKey),
                StatusMarkup(s.Status),
                dates,
                $"${s.ExpenseTotal:0.00}",
                $"${s.SelectedFlightTotal:0.00}",
                $"${s.SelectedLodgingTotal:0.00}",
                $"[yellow]${s.TotalPlannedCost:0.00}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private StaySummary? PickStay(string title)
    {
        var stays = _svc.GetStays();
        if (stays.Count == 0) { AnsiConsole.MarkupLine("[yellow]No stays yet.[/]"); Pause(); return null; }
        return AnsiConsole.Prompt(
            new SelectionPrompt<StaySummary>()
                .Title(title)
                .UseConverter(s => Markup.Escape(s.DisplayKey))
                .AddChoices(stays));
    }

    private void OnAddStay()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Add Stay[/] [grey](Esc to cancel)[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var city = ConsoleInput.AskOrEscape("City:");
        if (string.IsNullOrWhiteSpace(city)) return;
        var country = ConsoleInput.AskOrEscape("Country:");
        if (string.IsNullOrWhiteSpace(country)) return;

        var status = AnsiConsole.Prompt(
            new SelectionPrompt<StayStatus>()
                .Title("Status:")
                .UseConverter(s => s.ToString())
                .AddChoices(StayStatus.Idea, StayStatus.Shortlist, StayStatus.Locked));

        var start = PromptDate("Start date [grey](yyyy-MM-dd, blank to skip)[/]:");
        var end   = start.HasValue ? PromptDate("End date [grey](yyyy-MM-dd)[/]:") : null;

        try { _svc.AddStay(city, country, start, end, status); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnOpenStay()
    {
        var stay = PickStay("Select stay to open:");
        if (stay is null) return;
        new StayView(_svc, _ctx, stay).Run();
    }

    private void OnDeleteStay()
    {
        var stay = PickStay("Select stay to delete:");
        if (stay is null) return;
        if (!AnsiConsole.Confirm($"Delete [bold]{Markup.Escape(stay.DisplayKey)}[/]?")) return;
        try { _svc.DeleteStay(stay.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateBudget()
    {
        var current = _ctx.ActiveTrip?.TotalBudget.ToString("0.00") ?? "0.00";
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("New budget [grey](e.g. 6000)[/]:").DefaultValue(current));
        if (!decimal.TryParse(input, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var budget) || budget < 0)
        { AnsiConsole.MarkupLine("[red]Invalid budget.[/]"); Pause(); return; }
        try { _svc.UpdateTripBudget(budget); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnToggleWarnOnOverBudget()
    {
        var current = _ctx.ActiveTrip?.WarnOnOverBudget ?? false;
        var newValue = !current;
        try
        {
            _svc.SetWarnOnOverBudget(newValue);
            AnsiConsole.MarkupLine(newValue
                ? "[green]Over-budget warnings enabled.[/]"
                : "[grey]Over-budget warnings disabled.[/]");
            Pause();
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private bool OnArchive()
    {
        if (!AnsiConsole.Confirm("Archive this trip? It will be hidden from the trips list.")) return false;
        try { _svc.ArchiveActiveTrip(); return true; }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); return false; }
    }

    private void OnSetStatus()
    {
        var stay = PickStay("Select stay to set status:");
        if (stay is null) return;
        var status = AnsiConsole.Prompt(
            new SelectionPrompt<StayStatus>()
                .Title("Status:")
                .UseConverter(s => s.ToString())
                .AddChoices(StayStatus.Idea, StayStatus.Shortlist, StayStatus.Locked));
        try { _svc.SetStayStatus(stay.Id, status); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private static string StatusMarkup(StayStatus status) => status switch
    {
        StayStatus.Idea      => "[grey]Idea[/]",
        StayStatus.Shortlist => "[yellow]Shortlist[/]",
        StayStatus.Locked    => "[green]Locked[/]",
        _                    => status.ToString()
    };

    private static DateTime? PromptDate(string prompt)
    {
        while (true)
        {
            var input = ConsoleInput.AskOrEscape(prompt);
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (DateTime.TryParse(input, out var d)) return d;
            AnsiConsole.MarkupLine("[red]Invalid date — use yyyy-MM-dd.[/]");
        }
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(intercept: true);
    }
}
