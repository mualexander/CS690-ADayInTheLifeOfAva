using Spectre.Console;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

public class BookmarksView
{
    private readonly TripService _svc;
    private readonly StaySummary _stay;

    public BookmarksView(TripService svc, StaySummary stay)
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
                    .AddChoices("Add", "Rename", "Update URL", "Update Notes", "Delete", "Back"));

            switch (choice)
            {
                case "Add":          OnAdd();         break;
                case "Rename":       OnRename();      break;
                case "Update URL":   OnUpdateUrl();   break;
                case "Update Notes": OnUpdateNotes(); break;
                case "Delete":       OnDelete();      break;
                case "Back":         return;
            }
        }
    }

    private List<BookmarkSummary> GetBookmarks() => _svc.GetBookmarksForStay(_stay.Id).ToList();

    private void Render()
    {
        AnsiConsole.Write(new Rule($"[bold deepskyblue1]Bookmarks — {Markup.Escape(_stay.DisplayKey)}[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var bookmarks = GetBookmarks();
        if (bookmarks.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey italic]No bookmarks yet.[/]");
            AnsiConsole.WriteLine();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Title[/]")
            .AddColumn("[bold]URL[/]")
            .AddColumn("[bold]Notes[/]");

        foreach (var b in bookmarks)
            table.AddRow(
                Markup.Escape(b.Title),
                $"[link={b.Url}]{Markup.Escape(b.Url)}[/]",
                Markup.Escape(b.Notes ?? ""));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private BookmarkSummary? PickBookmark(string title)
    {
        var bookmarks = GetBookmarks();
        if (bookmarks.Count == 0) { AnsiConsole.MarkupLine("[yellow]No bookmarks.[/]"); Pause(); return null; }
        return AnsiConsole.Prompt(
            new SelectionPrompt<BookmarkSummary>()
                .Title(title)
                .UseConverter(b => Markup.Escape(b.Title))
                .AddChoices(bookmarks));
    }

    private void OnAdd()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Add Bookmark[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var title = AnsiConsole.Ask<string>("Title:");
        if (string.IsNullOrWhiteSpace(title)) return;
        var url = AnsiConsole.Ask<string>("URL:");
        if (string.IsNullOrWhiteSpace(url)) return;
        var notes = AnsiConsole.Prompt(
            new TextPrompt<string>("Notes [grey](optional)[/]:").AllowEmpty());

        try
        {
            _svc.AddBookmarkToStay(_stay.Id, title, url,
                string.IsNullOrWhiteSpace(notes) ? null : notes);
        }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnRename()
    {
        var b = PickBookmark("Select bookmark to rename:");
        if (b is null) return;
        var newTitle = AnsiConsole.Prompt(new TextPrompt<string>("New title:").DefaultValue(b.Title));
        if (string.IsNullOrWhiteSpace(newTitle)) return;
        try { _svc.UpdateBookmarkTitle(_stay.Id, b.Id, newTitle); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateUrl()
    {
        var b = PickBookmark("Select bookmark to update URL:");
        if (b is null) return;
        var newUrl = AnsiConsole.Prompt(new TextPrompt<string>("New URL:").DefaultValue(b.Url));
        if (string.IsNullOrWhiteSpace(newUrl)) return;
        try { _svc.UpdateBookmarkUrl(_stay.Id, b.Id, newUrl); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnUpdateNotes()
    {
        var b = PickBookmark("Select bookmark to update notes:");
        if (b is null) return;
        var notes = AnsiConsole.Prompt(
            new TextPrompt<string>("Notes [grey](blank to clear)[/]:")
                .DefaultValue(b.Notes ?? "")
                .AllowEmpty());
        try { _svc.UpdateBookmarkNotes(_stay.Id, b.Id, string.IsNullOrWhiteSpace(notes) ? null : notes); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private void OnDelete()
    {
        var b = PickBookmark("Select bookmark to delete:");
        if (b is null) return;
        if (!AnsiConsole.Confirm($"Delete [bold]{Markup.Escape(b.Title)}[/]?")) return;
        try { _svc.DeleteBookmark(_stay.Id, b.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(intercept: true);
    }
}
