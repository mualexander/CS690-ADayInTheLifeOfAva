using System.Text;
using Spectre.Console;

namespace TravelPlanner.Cli.Views;

/// <summary>
/// Escape-aware text input. All methods throw <see cref="OperationCanceledException"/>
/// when the user presses Escape, so callers only need a single catch at the menu level.
/// </summary>
internal static class ConsoleInput
{
    /// <summary>
    /// Renders <paramref name="markupPrompt"/> and reads a line of input.
    /// Returns the typed string (possibly empty) on Enter.
    /// If Enter is pressed with no input and <paramref name="defaultValue"/> is set, returns the default.
    /// Throws <see cref="OperationCanceledException"/> on Escape.
    /// </summary>
    public static string AskOrEscape(string markupPrompt, string? defaultValue = null)
    {
        var hint = defaultValue != null ? $" [grey]({Markup.Escape(defaultValue)})[/]" : "";
        AnsiConsole.Markup($"{markupPrompt}{hint} ");

        var sb = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    AnsiConsole.MarkupLine(" [grey](cancelled)[/]");
                    throw new OperationCanceledException();

                case ConsoleKey.Enter:
                    AnsiConsole.WriteLine();
                    var value = sb.ToString();
                    return value.Length == 0 && defaultValue != null ? defaultValue : value;

                case ConsoleKey.Backspace:
                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        sb.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                    break;
            }
        }
    }
}
