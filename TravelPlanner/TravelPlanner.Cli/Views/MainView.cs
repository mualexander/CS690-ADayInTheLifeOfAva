using System.Collections.ObjectModel;
using Terminal.Gui;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

/// <summary>Main screen: lists all trips and allows create/open/delete.</summary>
public class MainView : Window
{
    private readonly TripService _svc;
    private readonly InMemoryTripContext _ctx;

    private readonly ListView _listView;
    private readonly Label _lName, _lBudget, _lCost, _lLeft, _lStays;
    private List<TripSummary> _trips = new();
    private readonly ObservableCollection<string> _listSource = new();

    public MainView(TripService svc, InMemoryTripContext ctx) : base()
    {
        Title = "TravelPlanner";
        _svc = svc;
        _ctx = ctx;
        X = 0; Y = 0; Width = Dim.Fill(); Height = Dim.Fill();

        // Left panel — trip list
        var leftFrame = new FrameView { Title = "Trips", X = 0, Y = 0, Width = Dim.Percent(45), Height = Dim.Fill(3) };
        _listView = new ListView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AllowsMarking = false,
        };
        _listView.SetSource(_listSource);
        _listView.SelectedItemChanged += (_, _) => RefreshDetail();
        _listView.OpenSelectedItem    += (_, _) => OpenSelectedTrip();
        leftFrame.Add(_listView);
        Add(leftFrame);

        // Right panel — trip details
        var rightFrame = new FrameView { Title = "Details", X = Pos.Right(leftFrame), Y = 0, Width = Dim.Fill(), Height = Dim.Fill(3) };
        _lName   = new Label { Text = "", X = 1, Y = 0 };
        _lBudget = new Label { Text = "", X = 1, Y = 1 };
        _lCost   = new Label { Text = "", X = 1, Y = 2 };
        _lLeft   = new Label { Text = "", X = 1, Y = 3 };
        _lStays  = new Label { Text = "", X = 1, Y = 4 };
        rightFrame.Add(_lName, _lBudget, _lCost, _lLeft, _lStays);
        Add(rightFrame);

        // Button row
        var y = Pos.AnchorEnd(2);
        var btnNew    = new Button { Text = "_New",       X = 1,                       Y = y };
        var btnOpen   = new Button { Text = "_Open",      X = Pos.Right(btnNew)    + 1, Y = y };
        var btnDelete = new Button { Text = "_Delete",    X = Pos.Right(btnOpen)   + 1, Y = y };
        var btnSeed   = new Button { Text = "_Seed Demo", X = Pos.Right(btnDelete) + 1, Y = y };
        var btnQuit   = new Button { Text = "(Esc) Quit", X = Pos.AnchorEnd(14),        Y = y };

        btnNew.Accepting    += (_, _) => OnNew();
        btnOpen.Accepting   += (_, _) => OpenSelectedTrip();
        btnDelete.Accepting += (_, _) => OnDelete();
        btnSeed.Accepting   += (_, _) => OnSeed();
        btnQuit.Accepting   += (_, _) => Application.RequestStop();

        Add(btnNew, btnOpen, btnDelete, btnSeed, btnQuit);

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
                    case 'n': OnNew();               key.Handled = true; return;
                    case 'o': OpenSelectedTrip();    key.Handled = true; return;
                    case 'd': OnDelete();            key.Handled = true; return;
                    case 's': OnSeed();              key.Handled = true; return;
                }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private TripSummary? Selected =>
        _trips.Count > 0 && _listView.SelectedItem >= 0 && _listView.SelectedItem < _trips.Count
            ? _trips[_listView.SelectedItem]
            : null;

    private void Refresh()
    {
        _trips = _svc.GetTrips().ToList();
        _listSource.Clear();
        foreach (var t in _trips)
            _listSource.Add($" {t.Name}  [{t.StayCount} stays, ${t.TotalBudget:0}]");
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        var t = Selected;
        if (t is null)
        {
            _lName.Text   = "Select a trip to view details.";
            _lBudget.Text = _lCost.Text = _lLeft.Text = _lStays.Text = "";
            return;
        }
        _lName.Text   = $"Name:   {t.Name}";
        _lBudget.Text = $"Budget: {t.TotalBudget:0.00}";
        _lCost.Text   = $"Cost:   {t.TotalPlannedCost:0.00}";
        _lLeft.Text   = $"Left:   {t.RemainingBudget:0.00}";
        _lStays.Text  = $"Stays:  {t.StayCount}";
        SetNeedsDraw();
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void OpenSelectedTrip()
    {
        var t = Selected;
        if (t is null) { MessageBox.ErrorQuery("", "Select a trip first.", "OK"); return; }
        _svc.SelectTrip(t.Id);
        Application.Run(new TripView(_svc, _ctx));
        Refresh();
    }

    private void OnNew()
    {
        if (!DialogHelpers.PromptTwoFields("New Trip", "Name:", "", "Budget:", "", out var name, out var budgetStr))
            return;

        if (string.IsNullOrWhiteSpace(name))
        { MessageBox.ErrorQuery("Validation", "Name is required.", "OK"); return; }

        if (!decimal.TryParse(budgetStr, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var budget) || budget <= 0)
        { MessageBox.ErrorQuery("Validation", "Enter a valid budget (e.g. 5000).", "OK"); return; }

        try
        {
            var trip = _svc.CreateTrip(name, budget);
            _svc.SelectTrip(trip.Id);
            Refresh();
            Application.Run(new TripView(_svc, _ctx));
            Refresh();
        }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnDelete()
    {
        var t = Selected;
        if (t is null) { MessageBox.ErrorQuery("", "Select a trip first.", "OK"); return; }
        if (!DialogHelpers.Confirm("Delete Trip", $"Delete '{t.Name}'?\nThis cannot be undone.")) return;
        try { _svc.DeleteTrip(t.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnSeed()
    {
        try
        {
            var trip = _svc.CreateTrip("Seed: Japan 2026", 5000m);
            _svc.SelectTrip(trip.Id);
            _svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));
            _svc.AddStay("Osaka", "Japan", new DateTime(2026, 1, 14), new DateTime(2026, 1, 16));
            _svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 16), new DateTime(2026, 1, 20));
            var stays = _svc.GetStays();
            if (stays.Count > 0)
                _svc.AddExpenseToStay(stays[0].Id, "Meals", 180m, ExpenseCategory.Food, "Sushi + ramen");
            Refresh();
            MessageBox.Query("Done", "Demo data seeded.", "OK");
        }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }
}
