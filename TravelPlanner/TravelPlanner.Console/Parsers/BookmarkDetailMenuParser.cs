namespace TravelPlanner.ConsoleApp;

public static class BookmarkDetailMenuParser
{
    public static BookmarkDetailMenuCommand Parse(string? input)
    {
        var cmd = Normalize(input);

        return cmd switch
        {
            "v" or "view" => BookmarkDetailMenuCommand.ViewDetails,
            "r" or "rename" => BookmarkDetailMenuCommand.Rename,
            "u" or "url" => BookmarkDetailMenuCommand.UpdateUrl,
            "n" or "notes" => BookmarkDetailMenuCommand.UpdateNotes,
            "x" or "delete" => BookmarkDetailMenuCommand.Delete,
            "q" or "back" => BookmarkDetailMenuCommand.Back,
            _ => BookmarkDetailMenuCommand.Unknown
        };
    }

    private static string Normalize(string? input)
    {
        return (input ?? "").Trim().ToLowerInvariant();
    }
}