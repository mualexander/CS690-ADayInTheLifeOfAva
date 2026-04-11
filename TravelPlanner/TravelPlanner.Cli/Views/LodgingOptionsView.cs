using Spectre.Console;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

public class LodgingOptionsView
{
    private readonly TripService _svc;
    private readonly StaySummary _stay;

    public LodgingOptionsView(TripService svc, StaySummary stay)
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
                    .AddChoices("Add", "Update Price", "Update URL", "Update Rating", "Update Neighborhood", "Select", "Deselect", "Delete", "Back"));

            try
            {
                switch (choice)
                {
                    case "Add":                  OnAdd();                  break;
                    case "Update Price":         OnUpdatePrice();          break;
                    case "Update URL":           OnUpdateUrl();            break;
                    case "Update Rating":        OnUpdateRating();         break;
                    case "Update Neighborhood":  OnUpdateNeighborhood();   break;
                    case "Select":               OnMarkSelected();         break;
                    case "Deselect":             OnMarkDeselected();       break;
                    case "Delete":               OnDelete();               break;
                    case "Back":                 return;
                }
            }
            catch (OperationCanceledException) { }
        }
    }

    private List<LodgingOptionSummary> GetOptions() => _svc.GetLodgingOptionsForStay(_stay.Id).ToList();

    private void Render()
    {
        AnsiConsole.Write(new Rule($"[bold deepskyblue1]Lodging — {Markup.Escape(_stay.DisplayKey)}[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var options = GetOptions();
        if (options.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey italic]No lodging options yet.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var budget    = _svc.GetTripBudget();
        var remaining = _svc.GetTripRemainingBudget();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Property[/]")
            .AddColumn("[bold]Check-in[/]")
            .AddColumn("[bold]Check-out[/]")
            .AddColumn(new TableColumn("[bold]Price[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Rating[/]").RightAligned())
            .AddColumn("[bold]Link[/]")
            .AddColumn("[bold] [/]")
            .AddColumn("[bold]Checked At[/]");
        foreach (var l in options)
        {
            var overBudget = budget > 0 && !l.IsSelected && l.Price.HasValue && l.Price.Value > remaining;
            var price    = l.Price.HasValue
                ? (overBudget ? $"[red]${l.Price.Value:0.00}[/]" : $"[yellow]${l.Price.Value:0.00}[/]")
                : "[grey]?[/]";
            var rating   = l.Rating.HasValue ? $"{l.Rating.Value:0.0}" : "[grey]-[/]";
            var sel      = l.IsSelected ? "[green]✓[/]" : "";
            var property = string.IsNullOrWhiteSpace(l.Neighborhood)
                ? Markup.Escape(l.PropertyName)
                : $"[grey]{Markup.Escape(l.Neighborhood)}[/] · {Markup.Escape(l.PropertyName)}";
            table.AddRow(
                property,
                l.CheckInDate.ToString("yyyy-MM-dd"),
                l.CheckOutDate.ToString("yyyy-MM-dd"),
                price,
                rating,
                LinkMarkup(l.Url),
                sel,
                l.LastCheckedAt?.ToString("yyyy-MM-dd HH:mm") ?? ""
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private LodgingOptionSummary? PickOption(string title)
    {
        var options = GetOptions();
        if (options.Count == 0) { AnsiConsole.MarkupLine("[yellow]No lodging options.[/]"); Pause(); return null; }
        if (options.Count == 1) return options[0];
        return AnsiConsole.Prompt(
            new SelectionPrompt<LodgingOptionSummary>()
                .Title(title)
                .UseConverter(l =>
                    $"{Markup.Escape(l.PropertyName)}" +
                    $"  {l.CheckInDate:MM-dd}→{l.CheckOutDate:MM-dd}" +
                    (l.IsSelected ? "  [green]✓[/]" : ""))
                .AddChoices(options));
    }

    private void OnAdd()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Add Lodging Option[/] [grey](Esc to cancel)[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var url  = ConsoleInput.AskOrEscape("URL:");
        if (string.IsNullOrWhiteSpace(url)) return;
        var name = ConsoleInput.AskOrEscape("Property name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var checkIn  = PromptDate("Check-in [grey](yyyy-MM-dd)[/]:");
        if (!checkIn.HasValue) return;
        var checkOut = PromptDate("Check-out [grey](yyyy-MM-dd)[/]:");
        if (!checkOut.HasValue) return;

        var price        = PromptOptionalDecimal("Price [grey](blank = unknown)[/]:");
        var rating       = PromptOptionalDecimal("Rating [grey](0-5.0, blank = unknown)[/]:");
        var neighborhood = ConsoleInput.AskOrEscape("Neighborhood [grey](blank to skip)[/]:");
        var selected     = AnsiConsole.Confirm("Mark as selected?", defaultValue: false);

        try
        {
            _svc.AddLodgingOptionToStay(_stay.Id, url, name, checkIn.Value, checkOut.Value, price, rating, neighborhood);
            if (selected)
            {
                var id = _svc.GetLodgingOptionsForStay(_stay.Id).Last().Id;
                _svc.SelectLodgingOption(_stay.Id, id);
                BudgetWarning.ShowIfOverBudget(_svc);
            }
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdatePrice()
    {
        var l = PickOption("Select lodging to update price:");
        if (l is null) return;
        var price = PromptOptionalDecimal("Price [grey](blank = unknown)[/]:", l.Price);
        try
        {
            _svc.UpdateLodgingOptionPrice(_stay.Id, l.Id, price);
            if (l.IsSelected)
                BudgetWarning.ShowIfOverBudget(_svc);
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateUrl()
    {
        var l = PickOption("Select lodging to update URL:");
        if (l is null) return;
        var newUrl = AnsiConsole.Prompt(new TextPrompt<string>("New URL:").DefaultValue(l.Url));
        if (string.IsNullOrWhiteSpace(newUrl)) return;
        try { _svc.UpdateLodgingOptionUrl(_stay.Id, l.Id, newUrl); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnMarkSelected()
    {
        var l = PickOption("Select lodging to mark as selected:");
        if (l is null) return;
        try
        {
            _svc.SelectLodgingOption(_stay.Id, l.Id);
            BudgetWarning.ShowIfOverBudget(_svc);
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnMarkDeselected()
    {
        var l = PickOption("Select lodging to deselect:");
        if (l is null) return;
        try { _svc.DeselectLodgingOption(_stay.Id, l.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateRating()
    {
        var l = PickOption("Select lodging to update rating:");
        if (l is null) return;
        var rating = PromptOptionalDecimal("Rating [grey](0-5.0, blank = clear)[/]:", l.Rating);
        try { _svc.UpdateLodgingOptionRating(_stay.Id, l.Id, rating); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateNeighborhood()
    {
        var l = PickOption("Select lodging to update neighborhood:");
        if (l is null) return;
        var current = l.Neighborhood ?? "";
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("Neighborhood [grey](blank to clear)[/]:").DefaultValue(current).AllowEmpty());
        try { _svc.UpdateLodgingOptionNeighborhood(_stay.Id, l.Id, string.IsNullOrWhiteSpace(input) ? null : input); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnDelete()
    {
        var l = PickOption("Select lodging to delete:");
        if (l is null) return;
        if (!AnsiConsole.Confirm($"Delete [bold]{Markup.Escape(l.PropertyName)}[/]?")) return;
        try { _svc.DeleteLodgingOption(_stay.Id, l.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private static DateTime? PromptDate(string prompt)
    {
        while (true)
        {
            var input = ConsoleInput.AskOrEscape(prompt);
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (DateTime.TryParse(input, out var d)) return d;
            AnsiConsole.MarkupLine("[red]Use yyyy-MM-dd format.[/]");
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
