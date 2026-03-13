using TravelPlanner.Console.Commands;

namespace TravelPlanner.Console.Parsers;

public static class BookmarkMenuParser
{
    public static BookmarkMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "l" or "list" => BookmarkMenuCommand.ListBookmarks,
            "a" or "add" => BookmarkMenuCommand.AddBookmark,
            "d" or "delete" => BookmarkMenuCommand.DeleteBookmark,
            "q" or "back" => BookmarkMenuCommand.Back,
            _ => BookmarkMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input)
    {
        return (input ?? "").Trim().ToLowerInvariant();
    }
}