using System.Collections.ObjectModel;
using System.Globalization;
using Terminal.Gui;

namespace TravelPlanner.Cli.Views;

/// <summary>Reusable dialog helpers for text/date/decimal input and confirmations.</summary>
internal static class DialogHelpers
{
    /// <summary>Shows a single text-field dialog. Returns entered string, or null if cancelled.</summary>
    public static string? PromptText(string title, string label, string initial = "")
    {
        string? result = null;

        var lbl = new Label { Text = label, X = 1, Y = 1 };
        var tf  = new TextField { Text = initial, X = label.Length + 2, Y = 1, Width = Dim.Fill() - 2 };

        var okBtn = new Button { Text = "OK", IsDefault = true };
        okBtn.Accepting += (_, _) => { result = tf.Text?.Trim() ?? ""; Application.RequestStop(); };
        var cancelBtn = new Button { Text = "Cancel" };
        cancelBtn.Accepting += (_, _) => Application.RequestStop();

        var dialog = new Dialog { Title = title, Width = 60, Height = 7 };
        dialog.AddButton(cancelBtn);
        dialog.AddButton(okBtn);
        dialog.Add(lbl, tf);
        Application.Run(dialog);

        return result;
    }

    /// <summary>Shows a two-field dialog. Returns true if confirmed with non-empty values.</summary>
    public static bool PromptTwoFields(
        string title,
        string label1, string initial1,
        string label2, string initial2,
        out string value1, out string value2)
    {
        value1 = initial1;
        value2 = initial2;
        bool confirmed = false;
        string v1 = initial1, v2 = initial2;

        int col = Math.Max(label1.Length, label2.Length) + 2;

        var lbl1 = new Label { Text = label1, X = 1, Y = 1 };
        var tf1  = new TextField { Text = initial1, X = col, Y = 1, Width = Dim.Fill() - 2 };
        var lbl2 = new Label { Text = label2, X = 1, Y = 3 };
        var tf2  = new TextField { Text = initial2, X = col, Y = 3, Width = Dim.Fill() - 2 };

        var okBtn = new Button { Text = "OK", IsDefault = true };
        okBtn.Accepting += (_, _) =>
        {
            v1 = tf1.Text?.Trim() ?? "";
            v2 = tf2.Text?.Trim() ?? "";
            confirmed = true;
            Application.RequestStop();
        };
        var cancelBtn = new Button { Text = "Cancel" };
        cancelBtn.Accepting += (_, _) => Application.RequestStop();

        var dialog = new Dialog { Title = title, Width = 60, Height = 10 };
        dialog.AddButton(cancelBtn);
        dialog.AddButton(okBtn);
        dialog.Add(lbl1, tf1, lbl2, tf2);
        Application.Run(dialog);

        value1 = v1;
        value2 = v2;
        return confirmed;
    }

    /// <summary>Prompts for a date (YYYY-MM-DD). Returns null if cancelled or blank.</summary>
    public static DateTime? PromptDate(string title, string label, string initial = "")
    {
        var s = PromptText(title, $"{label} (YYYY-MM-DD):", initial);
        if (s == null || string.IsNullOrWhiteSpace(s)) return null;

        if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;

        MessageBox.ErrorQuery("Invalid Date", "Use format YYYY-MM-DD.", "OK");
        return null;
    }

    /// <summary>Prompts for a datetime (YYYY-MM-DD HH:mm). Returns null if cancelled.</summary>
    public static DateTime? PromptDateTime(string title, string label, string initial = "")
    {
        var s = PromptText(title, $"{label} (YYYY-MM-DD HH:mm):", initial);
        if (s == null) return null;

        if (DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;

        MessageBox.ErrorQuery("Invalid DateTime", "Use format YYYY-MM-DD HH:mm.", "OK");
        return null;
    }

    /// <summary>Prompts for an optional decimal. Returns null if blank or cancelled.</summary>
    public static decimal? PromptOptionalDecimal(string title, string label, string initial = "")
    {
        var s = PromptText(title, $"{label} (blank = unknown):", initial);
        if (s == null || string.IsNullOrWhiteSpace(s)) return null;

        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var val))
            return val;

        MessageBox.ErrorQuery("Invalid", "Enter a valid number.", "OK");
        return null;
    }

    /// <summary>Prompts user to pick one item from a list. Returns selected index or -1 on cancel.</summary>
    public static int PromptSelect(string title, IList<string> items)
    {
        if (items.Count == 0) return -1;

        int selected = 0;
        bool confirmed = false;

        var lv = new ListView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AllowsMarking = false,
        };
        lv.SetSource(new ObservableCollection<string>(items));
        lv.SelectedItemChanged += (_, args) => selected = args.Item;
        lv.OpenSelectedItem    += (_, _) => { confirmed = true; Application.RequestStop(); };

        var okBtn = new Button { Text = "OK", IsDefault = true };
        okBtn.Accepting += (_, _) => { confirmed = true; Application.RequestStop(); };
        var cancelBtn = new Button { Text = "Cancel" };
        cancelBtn.Accepting += (_, _) => Application.RequestStop();

        int height = Math.Min(items.Count + 5, 20);
        var dialog = new Dialog { Title = title, Width = 60, Height = height };
        dialog.AddButton(cancelBtn);
        dialog.AddButton(okBtn);
        dialog.Add(lv);
        Application.Run(dialog);

        return confirmed ? selected : -1;
    }

    /// <summary>Shows a yes/no confirmation. Returns true if user chose "Yes".</summary>
    public static bool Confirm(string title, string message) =>
        MessageBox.Query(title, message, "Yes", "No") == 0;
}
