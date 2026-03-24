using Spectre.Console;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

public class StayView
{
    private readonly TripService _svc;
    private readonly InMemoryTripContext _ctx;
    private StaySummary _stay;

    public StayView(TripService svc, InMemoryTripContext ctx, StaySummary stay)
    {
        _svc  = svc;
        _ctx  = ctx;
        _stay = stay;
    }

    public void Run()
    {
        while (true)
        {
            // Refresh stay data — sub-views may have changed totals
            var updated = _svc.GetStays().FirstOrDefault(s => s.Id == _stay.Id);
            if (updated is null) return; // was deleted from another path
            _stay = updated;

            AnsiConsole.Clear();
            Render();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Action:[/]")
                    .AddChoices(
                        "Expenses", "Bookmarks", "Flights", "Lodging",
                        "Edit Place", "Set Start Date", "Set End Date",
                        "Delete Stay", "Back"));

            switch (choice)
            {
                case "Expenses":       new ExpensesView(_svc, _stay).Run();          break;
                case "Bookmarks":      new BookmarksView(_svc, _stay).Run();         break;
                case "Flights":        new FlightOptionsView(_svc, _stay).Run();     break;
                case "Lodging":        new LodgingOptionsView(_svc, _stay).Run();    break;
                case "Edit Place":     OnEditPlace();                                break;
                case "Set Start Date": OnSetStartDate();                             break;
                case "Set End Date":   OnSetEndDate();                               break;
                case "Delete Stay":    if (OnDeleteStay()) return;                   break;
                case "Back":           return;
            }
        }
    }

    private void Render()
    {
        AnsiConsole.Write(new Rule($"[bold deepskyblue1]Stay: {Markup.Escape(_stay.DisplayKey)}[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var dates = _stay.StartDate.HasValue
            ? $"{_stay.StartDate:yyyy-MM-dd} → {_stay.EndDate:yyyy-MM-dd}"
            : "[grey](not set)[/]";

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("[bold]City:[/]",    Markup.Escape(_stay.City));
        grid.AddRow("[bold]Country:[/]", Markup.Escape(_stay.Country));
        grid.AddRow("[bold]Dates:[/]",   dates);
        grid.AddRow("", "");
        grid.AddRow("[bold]Total:[/]",   $"[yellow]${_stay.TotalPlannedCost:0.00}[/]");
        grid.AddRow("  Expenses:",       $"${_stay.ExpenseTotal:0.00}");
        grid.AddRow("  Flights:",        $"${_stay.SelectedFlightTotal:0.00}");
        grid.AddRow("  Lodging:",        $"${_stay.SelectedLodgingTotal:0.00}");

        // Show links for any selected flights and lodging
        var selectedFlights = _svc.GetFlightOptionsForStay(_stay.Id).Where(f => f.IsSelected).ToList();
        var selectedLodging = _svc.GetLodgingOptionsForStay(_stay.Id).Where(l => l.IsSelected).ToList();
        if (selectedFlights.Count > 0 || selectedLodging.Count > 0)
        {
            grid.AddRow("", "");
            grid.AddRow("[bold]Selected travel options:[/]", "");
            foreach (var f in selectedFlights)
                grid.AddRow(
                    $"  Flight: {Markup.Escape(f.FromAirportCode)}→{Markup.Escape(f.ToAirportCode)}",
                    LinkMarkup(f.Url));
            foreach (var l in selectedLodging)
                grid.AddRow(
                    $"  Lodging: {Markup.Escape(l.PropertyName)}",
                    LinkMarkup(l.Url));
        }

        AnsiConsole.Write(new Panel(grid).Border(BoxBorder.Rounded).BorderColor(Color.Grey));
        AnsiConsole.WriteLine();
    }

    private static string LinkMarkup(string url) =>
        string.IsNullOrWhiteSpace(url) ? "[grey](no url)[/]" : $"[link={url}]↗ Open[/]";

    private void OnEditPlace()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Edit Place[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var city = AnsiConsole.Prompt(
            new TextPrompt<string>("City:").DefaultValue(_stay.City));
        var country = AnsiConsole.Prompt(
            new TextPrompt<string>("Country:").DefaultValue(_stay.Country));

        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
        { AnsiConsole.MarkupLine("[red]City and Country are required.[/]"); Pause(); return; }

        try { _svc.UpdateStayPlace(_stay.Id, city, country); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnSetStartDate()
    {
        var current = _stay.StartDate?.ToString("yyyy-MM-dd");
        var prompt  = current is not null
            ? new TextPrompt<string>($"Start date [grey](yyyy-MM-dd)[/]:").DefaultValue(current)
            : new TextPrompt<string>("Start date [grey](yyyy-MM-dd)[/]:").AllowEmpty();
        var input = AnsiConsole.Prompt(prompt);
        if (string.IsNullOrWhiteSpace(input)) return;
        if (!DateTime.TryParse(input, out var date))
        { AnsiConsole.MarkupLine("[red]Invalid date.[/]"); Pause(); return; }
        try { _svc.UpdateStayStartDate(_stay.Id, date); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnSetEndDate()
    {
        var current = _stay.EndDate?.ToString("yyyy-MM-dd");
        var prompt  = current is not null
            ? new TextPrompt<string>($"End date [grey](yyyy-MM-dd)[/]:").DefaultValue(current)
            : new TextPrompt<string>("End date [grey](yyyy-MM-dd)[/]:").AllowEmpty();
        var input = AnsiConsole.Prompt(prompt);
        if (string.IsNullOrWhiteSpace(input)) return;
        if (!DateTime.TryParse(input, out var date))
        { AnsiConsole.MarkupLine("[red]Invalid date.[/]"); Pause(); return; }
        try { _svc.UpdateStayEndDate(_stay.Id, date); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private bool OnDeleteStay()
    {
        if (!AnsiConsole.Confirm($"Delete [bold]{Markup.Escape(_stay.DisplayKey)}[/]? This cannot be undone.")) return false;
        try { _svc.DeleteStay(_stay.Id); return true; }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); return false; }
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(intercept: true);
    }
}
