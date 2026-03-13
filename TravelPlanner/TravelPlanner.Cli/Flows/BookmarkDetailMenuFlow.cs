using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class BookmarkDetailMenuFlow
{
    public static AppMode Handle(
        TripService svc,
        StaySummary? activeStay,
        ref BookmarkSummary? activeBookmark)
    {
        if (activeStay is null)
        {
            MenuRenderer.ShowMessage("No active stay selected.");
            return AppMode.TripMenu;
        }

        if (activeBookmark is null)
        {
            MenuRenderer.ShowMessage("No active bookmark selected.");
            return AppMode.BookmarkMenu;
        }

        MenuRenderer.ShowBookmarkDetailMenu();
        var command = BookmarkDetailMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case BookmarkDetailMenuCommand.ViewDetails:
                MenuRenderer.ShowBookmarkDetails(activeBookmark);
                return AppMode.BookmarkDetailMenu;

            case BookmarkDetailMenuCommand.Rename:
                ConsolePrompts.RenameBookmark(svc, activeStay, ref activeBookmark);
                return activeBookmark is null ? AppMode.BookmarkMenu : AppMode.BookmarkDetailMenu;

            case BookmarkDetailMenuCommand.UpdateUrl:
                ConsolePrompts.UpdateBookmarkUrl(svc, activeStay, ref activeBookmark);
                return activeBookmark is null ? AppMode.BookmarkMenu : AppMode.BookmarkDetailMenu;

            case BookmarkDetailMenuCommand.UpdateNotes:
                ConsolePrompts.UpdateBookmarkNotes(svc, activeStay, ref activeBookmark);
                return activeBookmark is null ? AppMode.BookmarkMenu : AppMode.BookmarkDetailMenu;

            case BookmarkDetailMenuCommand.Delete:
                ConsolePrompts.DeleteActiveBookmark(svc, activeStay, ref activeBookmark);
                return AppMode.BookmarkMenu;

            case BookmarkDetailMenuCommand.Back:
                activeBookmark = null;
                return AppMode.BookmarkMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.BookmarkDetailMenu;
        }
    }
}