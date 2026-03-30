using Spectre.Console;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

public class ExpensesView
{
    private readonly TripService _svc;
    private readonly StaySummary _stay;

    public ExpensesView(TripService svc, StaySummary stay)
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
                    .AddChoices("Add", "Rename", "Update Amount", "Update Notes", "Delete", "Back"));

            switch (choice)
            {
                case "Add":           OnAdd();          break;
                case "Rename":        OnRename();       break;
                case "Update Amount": OnUpdateAmount(); break;
                case "Update Notes":  OnUpdateNotes();  break;
                case "Delete":        OnDelete();       break;
                case "Back":          return;
            }
        }
    }

    private List<ExpenseSummary> GetExpenses() => _svc.GetExpensesForStay(_stay.Id).ToList();

    private void Render()
    {
        AnsiConsole.Write(new Rule($"[bold deepskyblue1]Expenses — {Markup.Escape(_stay.DisplayKey)}[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var expenses = GetExpenses();
        if (expenses.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey italic]No expenses yet.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Category[/]")
            .AddColumn(new TableColumn("[bold]Amount[/]").RightAligned())
            .AddColumn("[bold]Notes[/]");

        decimal total = 0;
        foreach (var e in expenses)
        {
            total += e.Amount;
            table.AddRow(
                Markup.Escape(e.Name),
                Markup.Escape(e.Category.ToString()),
                $"[yellow]${e.Amount:0.00}[/]",
                Markup.Escape(e.Notes ?? "")
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"  [bold]Total: [yellow]${total:0.00}[/][/]");
        AnsiConsole.WriteLine();
    }

    private ExpenseSummary? PickExpense(string title)
    {
        var expenses = GetExpenses();
        if (expenses.Count == 0) { AnsiConsole.MarkupLine("[yellow]No expenses.[/]"); Pause(); return null; }
        return AnsiConsole.Prompt(
            new SelectionPrompt<ExpenseSummary>()
                .Title(title)
                .UseConverter(e => $"{Markup.Escape(e.Name)}  {Markup.Escape($"[{e.Category}]")}  ${e.Amount:0.00}")
                .AddChoices(expenses));
    }

    private void OnAdd()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Add Expense[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var name = AnsiConsole.Ask<string>("Name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var amtStr = AnsiConsole.Ask<string>("Amount [grey](e.g. 25.50)[/]:");
        if (!decimal.TryParse(amtStr, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount < 0)
        { AnsiConsole.MarkupLine("[red]Invalid amount.[/]"); Pause(); return; }

        var category = AnsiConsole.Prompt(
            new SelectionPrompt<ExpenseCategory>()
                .Title("Category:")
                .UseConverter(c => c.ToString())
                .AddChoices(Enum.GetValues<ExpenseCategory>()));

        var notes = AnsiConsole.Prompt(
            new TextPrompt<string>("Notes [grey](optional, blank to skip)[/]:").AllowEmpty());

        try
        {
            _svc.AddExpenseToStay(_stay.Id, name, amount, category,
                string.IsNullOrWhiteSpace(notes) ? null : notes);
            BudgetWarning.ShowIfOverBudget(_svc);
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnRename()
    {
        var e = PickExpense("Select expense to rename:");
        if (e is null) return;
        var newName = AnsiConsole.Prompt(new TextPrompt<string>("New name:").DefaultValue(e.Name));
        if (string.IsNullOrWhiteSpace(newName)) return;
        try { _svc.UpdateExpenseTitle(_stay.Id, e.Id, newName); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateAmount()
    {
        var e = PickExpense("Select expense to update amount:");
        if (e is null) return;
        var amtStr = AnsiConsole.Prompt(
            new TextPrompt<string>("New amount:").DefaultValue(e.Amount.ToString("0.00")));
        if (!decimal.TryParse(amtStr, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var amt) || amt < 0)
        { AnsiConsole.MarkupLine("[red]Invalid amount.[/]"); Pause(); return; }
        try { _svc.UpdateExpenseAmount(_stay.Id, e.Id, amt); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateNotes()
    {
        var e = PickExpense("Select expense to update notes:");
        if (e is null) return;
        var notes = AnsiConsole.Prompt(
            new TextPrompt<string>("Notes [grey](blank to clear)[/]:")
                .DefaultValue(e.Notes ?? "")
                .AllowEmpty());
        try { _svc.UpdateExpenseNotes(_stay.Id, e.Id, string.IsNullOrWhiteSpace(notes) ? null : notes); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnDelete()
    {
        var e = PickExpense("Select expense to delete:");
        if (e is null) return;
        if (!AnsiConsole.Confirm($"Delete [bold]{Markup.Escape(e.Name)}[/]?")) return;
        try { _svc.DeleteExpense(_stay.Id, e.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(intercept: true);
    }
}
