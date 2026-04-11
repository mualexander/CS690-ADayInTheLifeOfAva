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
                    .AddChoices("Add", "Rename", "Update URL", "Update Notes", "Manage Tags", "Delete", "Back"));

            try
            {
                switch (choice)
                {
                    case "Add":          OnAdd();         break;
                    case "Rename":       OnRename();      break;
                    case "Update URL":   OnUpdateUrl();   break;
                    case "Update Notes": OnUpdateNotes(); break;
                    case "Manage Tags":  OnManageTags();  break;
                    case "Delete":       OnDelete();      break;
                    case "Back":         return;
                }
            }
            catch (OperationCanceledException) { }
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
            .AddColumn("[bold]Notes[/]")
            .AddColumn("[bold]Tags[/]");

        foreach (var b in bookmarks)
        {
            var tagsDisplay = b.Tags.Count > 0
                ? string.Join(" ", b.Tags.Select(t => $"[deepskyblue1]#{Markup.Escape(t)}[/]"))
                : "[grey](none)[/]";
            table.AddRow(
                Markup.Escape(b.Title),
                $"[link={b.Url}]{Markup.Escape(b.Url)}[/]",
                Markup.Escape(b.Notes ?? ""),
                tagsDisplay);
        }

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
        AnsiConsole.Write(new Rule("[bold deepskyblue1]Add Bookmark[/] [grey](Esc to cancel)[/]").RuleStyle("deepskyblue1"));
        AnsiConsole.WriteLine();

        var title = ConsoleInput.AskOrEscape("Title:");
        if (string.IsNullOrWhiteSpace(title)) return;
        var url = ConsoleInput.AskOrEscape("URL:");
        if (string.IsNullOrWhiteSpace(url)) return;
        var notes = ConsoleInput.AskOrEscape("Notes [grey](optional)[/]:");

        var tags = PromptTags(_svc.GetAllTagsForActiveTrip(), []);

        try
        {
            _svc.AddBookmarkToStay(_stay.Id, title, url,
                string.IsNullOrWhiteSpace(notes) ? null : notes,
                tags.Count > 0 ? tags : null);
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

    private void OnManageTags()
    {
        var b = PickBookmark("Select bookmark to manage tags:");
        if (b is null) return;

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold deepskyblue1]Tags — {Markup.Escape(b.Title)}[/]").RuleStyle("deepskyblue1"));
            AnsiConsole.WriteLine();

            // Refresh to pick up changes from previous iteration
            b = GetBookmarks().FirstOrDefault(x => x.Id == b.Id);
            if (b is null) return;

            var currentTags = b.Tags.Count > 0
                ? string.Join(" ", b.Tags.Select(t => $"[deepskyblue1]#{Markup.Escape(t)}[/]"))
                : "[grey](none)[/]";
            AnsiConsole.MarkupLine($"  Current tags: {currentTags}");
            AnsiConsole.WriteLine();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Action:[/]")
                    .AddChoices("Add Tags", "Remove Tags", "Back"));

            if (action == "Back") return;

            if (action == "Add Tags")
            {
                var suggestions = _svc.GetAllTagsForActiveTrip()
                    .Except(b.Tags, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var toAdd = PromptTags(suggestions, b.Tags);
                foreach (var tag in toAdd)
                {
                    try { _svc.AddTagToBookmark(_stay.Id, b.Id, tag); }
                    catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
                }
            }
            else // Remove Tags
            {
                if (b.Tags.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No tags to remove.[/]");
                    Pause();
                    continue;
                }

                var toRemove = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("Select tags to remove [grey](space to toggle, enter to confirm)[/]:")
                        .NotRequired()
                        .AddChoices(b.Tags));

                foreach (var tag in toRemove)
                {
                    try { _svc.RemoveTagFromBookmark(_stay.Id, b.Id, tag); }
                    catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
                }
            }
        }
    }

    private void OnDelete()
    {
        var b = PickBookmark("Select bookmark to delete:");
        if (b is null) return;
        if (!AnsiConsole.Confirm($"Delete [bold]{Markup.Escape(b.Title)}[/]?")) return;
        try { _svc.DeleteBookmark(_stay.Id, b.Id); }
        catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]"); Pause(); }
    }

    /// <summary>
    /// Prompts the user to select from existing tag suggestions and/or type new tags.
    /// Returns the combined list of selected/entered tags (already normalized).
    /// </summary>
    /// <param name="suggestions">Tags to offer as pre-existing choices.</param>
    /// <param name="exclude">Tags to exclude from suggestions (already on the bookmark).</param>
    private static List<string> PromptTags(IEnumerable<string> suggestions, IEnumerable<string> exclude)
    {
        var selected = new List<string>();
        var choices = suggestions
            .Except(exclude, StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        if (choices.Count > 0)
        {
            var picks = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select existing tags [grey](space to toggle, enter to confirm)[/]:")
                    .NotRequired()
                    .AddChoices(choices));
            selected.AddRange(picks);
        }

        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("New tag [grey](blank to finish)[/]:").AllowEmpty());
            if (string.IsNullOrWhiteSpace(input)) break;
            var normalized = input.Trim().ToLowerInvariant();
            if (!selected.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                selected.Add(normalized);
        }

        return selected;
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key...[/]");
        Console.ReadKey(intercept: true);
    }
}
