using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class BookmarkMenuFlow
{
    public static AppMode Handle(TripService svc, StaySummary? activeStay, ref BookmarkSummary? activeBookmark)
    {
        if (activeStay is null)
        {
            MenuRenderer.ShowMessage("No active stay selected.");
            return AppMode.TripMenu;
        }

        MenuRenderer.ShowBookmarkMenu();
        var command = BookmarkMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case BookmarkMenuCommand.ListBookmarks:
                MenuRenderer.ShowBookmarks(svc.GetBookmarksForStay(activeStay.Id));
                return AppMode.BookmarkMenu;

            case BookmarkMenuCommand.AddBookmark:
                ConsolePrompts.AddBookmark(svc, activeStay);
                return AppMode.BookmarkMenu;

            case BookmarkMenuCommand.SelectBookmark:
                activeBookmark = ConsolePrompts.SelectBookmark(svc, activeStay);
                return AppMode.BookmarkDetailMenu;

            case BookmarkMenuCommand.DeleteBookmark:
                ConsolePrompts.DeleteBookmark(svc, activeStay);
                activeBookmark = null;
                return AppMode.BookmarkMenu;

            case BookmarkMenuCommand.Back:
                activeBookmark = null;
                return AppMode.StayMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.BookmarkMenu;
        }
    }
}