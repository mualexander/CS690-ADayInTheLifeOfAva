using Spectre.Console;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

public class FlightOptionsView
{
    private readonly TripService _svc;
    private readonly StaySummary _stay;

    public FlightOptionsView(TripService svc, StaySummary stay)
    {
        _svc  = svc;
        _stay = stay;
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
                    .AddChoices("Add", "Update Price", "Update URL", "Select", "Deselect", "Delete", "Back"));

            try
            {
                switch (choice)
                {
                    case "Add":          OnAdd();            break;
                    case "Update Price": OnUpdatePrice();    break;
                    case "Update URL":   OnUpdateUrl();      break;
                    case "Select":       OnMarkSelected();   break;
                    case "Deselect":     OnMarkDeselected(); break;
                    case "Delete":       OnDelete();         break;
                    case "Back":         return;
                }
            }
            catch (OperationCanceledException) { }
        }
    }

    private List<FlightOptionSummary> GetOptions() => _svc.GetFlightOptionsForStay(_stay.Id).ToList();

    private void Render()
    {
        AnsiConsole.Write(new Rule($"[bold deepskyblue1]Flights — {Markup.Escape(_stay.DisplayKey)}[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var options = GetOptions();
        if (options.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey italic]No flight options yet.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var budget    = _svc.GetTripBudget();
        var remaining = _svc.GetTripRemainingBudget();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Route[/]")
            .AddColumn("[bold]Depart[/]")
            .AddColumn("[bold]Arrive[/]")
            .AddColumn(new TableColumn("[bold]Price[/]").RightAligned())
            .AddColumn("[bold]Link[/]")
            .AddColumn("[bold] [/]")
            .AddColumn("[bold]Checked At[/]");

        foreach (var f in options)
        {
            var overBudget = budget > 0 && !f.IsSelected && f.Price.HasValue && f.Price.Value > remaining;
            var price = f.Price.HasValue
                ? (overBudget ? $"[red]${f.Price.Value:0.00}[/]" : $"[yellow]${f.Price.Value:0.00}[/]")
                : "[grey]?[/]";
            var sel   = f.IsSelected ? "[green]✓[/]" : "";
            table.AddRow(
                $"{Markup.Escape(f.FromAirportCode)} → {Markup.Escape(f.ToAirportCode)}",
                f.DepartTime.ToString("yyyy-MM-dd HH:mm"),
                f.ArriveTime.ToString("yyyy-MM-dd HH:mm"),
                price,
                LinkMarkup(f.Url),
                sel,
                f.LastCheckedAt?.ToString("yyyy-MM-dd HH:mm") ?? ""
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private FlightOptionSummary? PickOption(string title)
    {
        var options = GetOptions();
        if (options.Count == 0) { AnsiConsole.MarkupLine("[yellow]No flight options.[/]"); Pause(); return null; }
        if (options.Count == 1) return options[0];
        return AnsiConsole.Prompt(
            new SelectionPrompt<FlightOptionSummary>()
                .Title(title)
                .UseConverter(f =>
                    $"{Markup.Escape(f.FromAirportCode)}→{Markup.Escape(f.ToAirportCode)}" +
                    $"  {f.DepartTime:MM-dd HH:mm}" +
                    (f.IsSelected ? "  [green]✓[/]" : ""))
                .AddChoices(options));
    }

    private void OnAdd()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Add Flight Option[/] [grey](Esc to cancel)[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var homeAirport = _svc.GetTripHomeAirportCode();

        var url  = ConsoleInput.AskOrEscape("URL:");
        if (string.IsNullOrWhiteSpace(url)) return;
        var from = ConsoleInput.AskOrEscape("From airport [grey](e.g. JFK)[/]:", homeAirport);
        if (string.IsNullOrWhiteSpace(from)) return;
        var to   = ConsoleInput.AskOrEscape("To airport [grey](e.g. NRT)[/]:", homeAirport);
        if (string.IsNullOrWhiteSpace(to)) return;

        var depart = PromptDateTime("Depart [grey](yyyy-MM-dd HH:mm)[/]:");
        if (!depart.HasValue) return;
        var arrive = PromptDateTime("Arrive [grey](yyyy-MM-dd HH:mm)[/]:");
        if (!arrive.HasValue) return;

        var price    = PromptOptionalDecimal("Price [grey](blank = unknown)[/]:");
        var selected = AnsiConsole.Confirm("Mark as selected?", defaultValue: false);

        try
        {
            _svc.AddFlightOptionToStay(_stay.Id, url, from, to, depart.Value, arrive.Value, price);
            if (selected)
            {
                var id = _svc.GetFlightOptionsForStay(_stay.Id).Last().Id;
                _svc.SelectFlightOption(_stay.Id, id);
                BudgetWarning.ShowIfOverBudget(_svc);
            }
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdatePrice()
    {
        var f = PickOption("Select flight to update price:");
        if (f is null) return;
        var price = PromptOptionalDecimal("Price [grey](blank = unknown)[/]:", f.Price);
        try
        {
            _svc.UpdateFlightOptionPrice(_stay.Id, f.Id, price);
            if (f.IsSelected)
                BudgetWarning.ShowIfOverBudget(_svc);
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateUrl()
    {
        var f = PickOption("Select flight to update URL:");
        if (f is null) return;
        var newUrl = AnsiConsole.Prompt(new TextPrompt<string>("New URL:").DefaultValue(f.Url));
        if (string.IsNullOrWhiteSpace(newUrl)) return;
        try { _svc.UpdateFlightOptionUrl(_stay.Id, f.Id, newUrl); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnMarkSelected()
    {
        var f = PickOption("Select flight to mark as selected:");
        if (f is null) return;
        try
        {
            _svc.SelectFlightOption(_stay.Id, f.Id);
            BudgetWarning.ShowIfOverBudget(_svc);
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnMarkDeselected()
    {
        var f = PickOption("Select flight to deselect:");
        if (f is null) return;
        try { _svc.DeselectFlightOption(_stay.Id, f.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnDelete()
    {
        var f = PickOption("Select flight to delete:");
        if (f is null) return;
        if (!AnsiConsole.Confirm(
            $"Delete {Markup.Escape(f.FromAirportCode)}→{Markup.Escape(f.ToAirportCode)}?")) return;
        try { _svc.DeleteFlightOption(_stay.Id, f.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private static DateTime? PromptDateTime(string prompt)
    {
        while (true)
        {
            var input = ConsoleInput.AskOrEscape(prompt);
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (DateTime.TryParse(input, out var d)) return d;
            AnsiConsole.MarkupLine("[red]Use yyyy-MM-dd HH:mm format.[/]");
        }
    }

    private static decimal? PromptOptionalDecimal(string prompt, decimal? current = null)
    {
        var input = ConsoleInput.AskOrEscape(prompt, current?.ToString("0.00"));
        if (string.IsNullOrWhiteSpace(input)) return null;
        if (decimal.TryParse(input, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
        AnsiConsole.MarkupLine("[red]Invalid amount.[/]");
        return null;
    }

    private static string LinkMarkup(string url) =>
        string.IsNullOrWhiteSpace(url) ? "[grey](no url)[/]" : $"[link={url}]↗ Open[/]";

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(intercept: true);
    }
}
