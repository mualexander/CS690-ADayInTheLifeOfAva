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
                    .AddChoices("Add", "Update Price", "Update URL", "Select", "Deselect", "Delete", "Back"));

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

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Property[/]")
            .AddColumn("[bold]Check-in[/]")
            .AddColumn("[bold]Check-out[/]")
            .AddColumn(new TableColumn("[bold]Price[/]").RightAligned())
            .AddColumn("[bold]Link[/]")
            .AddColumn("[bold] [/]");

        foreach (var l in options)
        {
            var price = l.Price.HasValue ? $"[yellow]${l.Price.Value:0.00}[/]" : "[grey]?[/]";
            var sel   = l.IsSelected ? "[green]✓[/]" : "";
            table.AddRow(
                Markup.Escape(l.PropertyName),
                l.CheckInDate.ToString("yyyy-MM-dd"),
                l.CheckOutDate.ToString("yyyy-MM-dd"),
                price,
                LinkMarkup(l.Url),
                sel
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private LodgingOptionSummary? PickOption(string title)
    {
        var options = GetOptions();
        if (options.Count == 0) { AnsiConsole.MarkupLine("[yellow]No lodging options.[/]"); Pause(); return null; }
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
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Add Lodging Option[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var url  = AnsiConsole.Ask<string>("URL:");
        if (string.IsNullOrWhiteSpace(url)) return;
        var name = AnsiConsole.Ask<string>("Property name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var checkIn  = PromptDate("Check-in [grey](yyyy-MM-dd)[/]:");
        if (!checkIn.HasValue) return;
        var checkOut = PromptDate("Check-out [grey](yyyy-MM-dd)[/]:");
        if (!checkOut.HasValue) return;

        var price = PromptOptionalDecimal("Price [grey](blank = unknown)[/]:");

        try { _svc.AddLodgingOptionToStay(_stay.Id, url, name, checkIn.Value, checkOut.Value, price); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdatePrice()
    {
        var l = PickOption("Select lodging to update price:");
        if (l is null) return;
        var price = PromptOptionalDecimal("Price [grey](blank = unknown)[/]:", l.Price);
        try { _svc.UpdateLodgingOptionPrice(_stay.Id, l.Id, price); }
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
        try { _svc.SelectLodgingOption(_stay.Id, l.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnMarkDeselected()
    {
        var l = PickOption("Select lodging to deselect:");
        if (l is null) return;
        try { _svc.DeselectLodgingOption(_stay.Id, l.Id); }
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
            var input = AnsiConsole.Prompt(new TextPrompt<string>(prompt).AllowEmpty());
            if (string.IsNullOrWhiteSpace(input)) return null;
            if (DateTime.TryParse(input, out var d)) return d;
            AnsiConsole.MarkupLine("[red]Use yyyy-MM-dd format.[/]");
        }
    }

    private static decimal? PromptOptionalDecimal(string prompt, decimal? current = null)
    {
        var tp = new TextPrompt<string>(prompt).AllowEmpty();
        if (current.HasValue) tp = tp.DefaultValue(current.Value.ToString("0.00"));
        var input = AnsiConsole.Prompt(tp);
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
