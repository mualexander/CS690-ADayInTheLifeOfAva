using System.Collections.ObjectModel;
using Terminal.Gui;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

/// <summary>Expense management for a stay.</summary>
public class ExpensesView : Window
{
    private readonly TripService _svc;
    private StaySummary _stay;

    private readonly ListView _listView;
    private readonly Label _lName, _lAmount, _lCategory, _lNotes, _lCreated;
    private List<ExpenseSummary> _expenses = new();
    private readonly ObservableCollection<string> _listSource = new();

    public ExpensesView(TripService svc, StaySummary stay) : base()
    {
        Title = "Expenses";
        _svc  = svc;
        _stay = stay;
        X = 0; Y = 0; Width = Dim.Fill(); Height = Dim.Fill();

        // Left — expense list
        var leftFrame = new FrameView { Title = "Expenses", X = 0, Y = 0, Width = Dim.Percent(50), Height = Dim.Fill(3) };
        _listView = new ListView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AllowsMarking = false,
        };
        _listView.SetSource(_listSource);
        _listView.SelectedItemChanged += (_, _) => RefreshDetail();
        leftFrame.Add(_listView);
        Add(leftFrame);

        // Right — expense detail
        var rightFrame = new FrameView { Title = "Details", X = Pos.Right(leftFrame), Y = 0, Width = Dim.Fill(), Height = Dim.Fill(3) };
        _lName     = new Label { Text = "", X = 1, Y = 0 };
        _lAmount   = new Label { Text = "", X = 1, Y = 1 };
        _lCategory = new Label { Text = "", X = 1, Y = 2 };
        _lNotes    = new Label { Text = "", X = 1, Y = 3 };
        _lCreated  = new Label { Text = "", X = 1, Y = 5 };
        rightFrame.Add(_lName, _lAmount, _lCategory, _lNotes, _lCreated);
        Add(rightFrame);

        // Buttons
        var y = Pos.AnchorEnd(2);
        var btnAdd    = new Button { Text = "_Add",     X = 1,                       Y = y };
        var btnRename = new Button { Text = "_Rename",  X = Pos.Right(btnAdd)    + 1, Y = y };
        var btnAmount = new Button { Text = "A_mount",  X = Pos.Right(btnRename) + 1, Y = y };
        var btnNotes  = new Button { Text = "_Notes",   X = Pos.Right(btnAmount) + 1, Y = y };
        var btnDelete = new Button { Text = "_Delete",  X = Pos.Right(btnNotes)  + 1, Y = y };
        var btnBack   = new Button { Text = "(Esc) Back", X = Pos.AnchorEnd(14),       Y = y };

        btnAdd.Accepting    += (_, _) => OnAdd();
        btnRename.Accepting += (_, _) => OnRename();
        btnAmount.Accepting += (_, _) => OnUpdateAmount();
        btnNotes.Accepting  += (_, _) => OnUpdateNotes();
        btnDelete.Accepting += (_, _) => OnDelete();
        btnBack.Accepting   += (_, _) => Application.RequestStop();

        Add(btnAdd, btnRename, btnAmount, btnNotes, btnDelete, btnBack);

        Refresh();
        Application.KeyDown += OnAppKeyDown;
        Closed += (_, _) => Application.KeyDown -= OnAppKeyDown;
        Application.Invoke(() => _listView.SetFocus());
    }

    // ── Key handling ──────────────────────────────────────────────────────────

    private void OnAppKeyDown(object? sender, Key key)
    {
        if (Application.Top != this) return;
        if (!key.IsAlt && !key.IsCtrl)
        {
            if (key.KeyCode == KeyCode.Esc) { Application.RequestStop(); key.Handled = true; return; }
            if (key.AsRune.Value != 0)
                switch (char.ToLower((char)key.AsRune.Value))
                {
                    case 'a': OnAdd();           key.Handled = true; return;
                    case 'r': OnRename();        key.Handled = true; return;
                    case 'm': OnUpdateAmount();  key.Handled = true; return;
                    case 'n': OnUpdateNotes();   key.Handled = true; return;
                    case 'd': OnDelete();        key.Handled = true; return;
                }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private ExpenseSummary? Selected =>
        _expenses.Count > 0 && _listView.SelectedItem >= 0 && _listView.SelectedItem < _expenses.Count
            ? _expenses[_listView.SelectedItem]
            : null;

    private void Refresh()
    {
        Title = $"Expenses — {_stay.DisplayKey}";
        _expenses = _svc.GetExpensesForStay(_stay.Id).ToList();
        _listSource.Clear();
        foreach (var e in _expenses)
            _listSource.Add($" {e.Name}  [{e.Category}]  ${e.Amount:0.00}");
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        var e = Selected;
        if (e is null)
        {
            _lName.Text = "Select an expense to view details.";
            _lAmount.Text = _lCategory.Text = _lNotes.Text = _lCreated.Text = "";
            return;
        }
        _lName.Text     = $"Name:     {e.Name}";
        _lAmount.Text   = $"Amount:   {e.Amount:0.00}";
        _lCategory.Text = $"Category: {e.Category}";
        _lNotes.Text    = $"Notes:    {e.Notes ?? "(none)"}";
        _lCreated.Text  = $"Created:  {e.CreatedAt:yyyy-MM-dd HH:mm} UTC";
        SetNeedsDraw();
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void OnAdd()
    {
        var name = DialogHelpers.PromptText("Add Expense", "Name:", "");
        if (string.IsNullOrWhiteSpace(name)) return;

        var amtStr = DialogHelpers.PromptText("Add Expense", "Amount (e.g. 25.50):", "");
        if (!decimal.TryParse(amtStr, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount < 0)
        { MessageBox.ErrorQuery("Invalid", "Enter a valid amount.", "OK"); return; }

        var categories = Enum.GetValues<ExpenseCategory>();
        var catLabels  = categories.Select(c => c.ToString()).ToList();
        int catIdx     = DialogHelpers.PromptSelect("Select Category", catLabels);
        if (catIdx < 0) return;

        var notes = DialogHelpers.PromptText("Add Expense", "Notes (optional):", "");

        try
        {
            _svc.AddExpenseToStay(_stay.Id, name, amount, categories[catIdx], string.IsNullOrWhiteSpace(notes) ? null : notes);
            Refresh();
        }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnRename()
    {
        var e = Selected;
        if (e is null) { MessageBox.ErrorQuery("", "Select an expense first.", "OK"); return; }
        var newName = DialogHelpers.PromptText("Rename Expense", "New name:", e.Name);
        if (string.IsNullOrWhiteSpace(newName)) return;
        try { _svc.UpdateExpenseTitle(_stay.Id, e.Id, newName); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnUpdateAmount()
    {
        var e = Selected;
        if (e is null) { MessageBox.ErrorQuery("", "Select an expense first.", "OK"); return; }
        var amtStr = DialogHelpers.PromptText("Update Amount", "New amount:", e.Amount.ToString("0.00"));
        if (!decimal.TryParse(amtStr, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var amt) || amt < 0)
        { MessageBox.ErrorQuery("Invalid", "Enter a valid amount.", "OK"); return; }
        try { _svc.UpdateExpenseAmount(_stay.Id, e.Id, amt); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnUpdateNotes()
    {
        var e = Selected;
        if (e is null) { MessageBox.ErrorQuery("", "Select an expense first.", "OK"); return; }
        var notes = DialogHelpers.PromptText("Update Notes", "Notes (blank clears):", e.Notes ?? "");
        if (notes == null) return;
        try { _svc.UpdateExpenseNotes(_stay.Id, e.Id, string.IsNullOrWhiteSpace(notes) ? null : notes); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnDelete()
    {
        var e = Selected;
        if (e is null) { MessageBox.ErrorQuery("", "Select an expense first.", "OK"); return; }
        if (!DialogHelpers.Confirm("Delete Expense", $"Delete '{e.Name}'?")) return;
        try { _svc.DeleteExpense(_stay.Id, e.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }
}
