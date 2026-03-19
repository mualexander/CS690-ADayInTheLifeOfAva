using System.Collections.ObjectModel;
using Terminal.Gui;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

/// <summary>Shows stays for the active trip and allows stay management.</summary>
public class TripView : Window
{
    private readonly TripService _svc;
    private readonly InMemoryTripContext _ctx;

    private readonly ListView _listView;
    private readonly Label _lCity, _lCountry, _lDates, _lCost, _lExpenses, _lFlights, _lLodging;
    private readonly Label _headerLabel;
    private List<StaySummary> _stays = new();
    private readonly ObservableCollection<string> _listSource = new();

    public TripView(TripService svc, InMemoryTripContext ctx) : base()
    {
        Title = "Trip";
        _svc = svc;
        _ctx = ctx;
        X = 0; Y = 0; Width = Dim.Fill(); Height = Dim.Fill();

        // Header — trip budget summary
        var headerFrame = new FrameView { Title = "Trip", X = 0, Y = 0, Width = Dim.Fill(), Height = 3 };
        _headerLabel = new Label { Text = "", X = 1, Y = 0 };
        headerFrame.Add(_headerLabel);
        Add(headerFrame);

        // Left panel — stay list
        var leftFrame = new FrameView
        {
            Title = "Stays",
            X = 0, Y = Pos.Bottom(headerFrame),
            Width = Dim.Percent(45),
            Height = Dim.Fill(3),
        };
        _listView = new ListView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AllowsMarking = false,
        };
        _listView.SetSource(_listSource);
        _listView.SelectedItemChanged += (_, _) => RefreshDetail();
        _listView.OpenSelectedItem    += (_, _) => OpenSelectedStay();
        leftFrame.Add(_listView);
        Add(leftFrame);

        // Right panel — stay details
        var rightFrame = new FrameView
        {
            Title = "Stay Details",
            X = Pos.Right(leftFrame), Y = Pos.Bottom(headerFrame),
            Width = Dim.Fill(),
            Height = Dim.Fill(3),
        };
        _lCity     = new Label { Text = "", X = 1, Y = 0 };
        _lCountry  = new Label { Text = "", X = 1, Y = 1 };
        _lDates    = new Label { Text = "", X = 1, Y = 2 };
        _lCost     = new Label { Text = "", X = 1, Y = 3 };
        _lExpenses = new Label { Text = "", X = 1, Y = 4 };
        _lFlights  = new Label { Text = "", X = 1, Y = 5 };
        _lLodging  = new Label { Text = "", X = 1, Y = 6 };
        rightFrame.Add(_lCity, _lCountry, _lDates, _lCost, _lExpenses, _lFlights, _lLodging);
        Add(rightFrame);

        // Button row
        var y = Pos.AnchorEnd(2);
        var btnAdd     = new Button { Text = "_Add Stay",    X = 1,                       Y = y };
        var btnOpen    = new Button { Text = "_Open Stay",   X = Pos.Right(btnAdd)    + 1, Y = y };
        var btnDel     = new Button { Text = "_Delete Stay", X = Pos.Right(btnOpen)   + 1, Y = y };
        var btnArchive = new Button { Text = "Archi_ve",     X = Pos.Right(btnDel)    + 1, Y = y };
        var btnBack    = new Button { Text = "(Esc) Back",    X = Pos.AnchorEnd(14),       Y = y };

        btnAdd.Accepting     += (_, _) => OnAddStay();
        btnOpen.Accepting    += (_, _) => OpenSelectedStay();
        btnDel.Accepting     += (_, _) => OnDeleteStay();
        btnArchive.Accepting += (_, _) => OnArchive();
        btnBack.Accepting    += (_, _) => Application.RequestStop();

        Add(btnAdd, btnOpen, btnDel, btnArchive, btnBack);

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
                    case 'a': OnAddStay();        key.Handled = true; return;
                    case 'o': OpenSelectedStay(); key.Handled = true; return;
                    case 'd': OnDeleteStay();     key.Handled = true; return;
                    case 'v': OnArchive();        key.Handled = true; return;
                }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private StaySummary? Selected =>
        _stays.Count > 0 && _listView.SelectedItem >= 0 && _listView.SelectedItem < _stays.Count
            ? _stays[_listView.SelectedItem]
            : null;

    private void Refresh()
    {
        var trip = _ctx.ActiveTrip;
        if (trip is not null)
        {
            Title = $"Trip: {trip.Name}";
            _headerLabel.Text =
                $"Budget: {trip.TotalBudget:0.00}  |  " +
                $"Cost: {trip.TotalPlannedCost():0.00}  |  " +
                $"Left: {trip.RemainingBudget():0.00}";
        }

        _stays = _svc.GetStays().ToList();
        _listSource.Clear();
        foreach (var s in _stays)
            _listSource.Add($" {s.DisplayKey}  [${s.TotalPlannedCost:0}]");
        RefreshDetail();
        SetNeedsDraw();
    }

    private void RefreshDetail()
    {
        var s = Selected;
        if (s is null)
        {
            _lCity.Text     = "Select a stay to view details.";
            _lCountry.Text  = _lDates.Text = _lCost.Text = "";
            _lExpenses.Text = _lFlights.Text = _lLodging.Text = "";
            return;
        }
        _lCity.Text     = $"City:      {s.City}";
        _lCountry.Text  = $"Country:   {s.Country}";
        _lDates.Text    = s.StartDate.HasValue
            ? $"Dates:     {s.StartDate:yyyy-MM-dd} → {s.EndDate:yyyy-MM-dd}"
            : "Dates:     (not set)";
        _lCost.Text     = $"Total:     {s.TotalPlannedCost:0.00}";
        _lExpenses.Text = $"  Expenses:  {s.ExpenseTotal:0.00}";
        _lFlights.Text  = $"  Flights:   {s.SelectedFlightTotal:0.00}";
        _lLodging.Text  = $"  Lodging:   {s.SelectedLodgingTotal:0.00}";
        SetNeedsDraw();
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void OpenSelectedStay()
    {
        var s = Selected;
        if (s is null) { MessageBox.ErrorQuery("", "Select a stay first.", "OK"); return; }
        Application.Run(new StayView(_svc, _ctx, s));
        Refresh();
    }

    private void OnAddStay()
    {
        if (!DialogHelpers.PromptTwoFields("Add Stay", "City:", "", "Country:", "", out var city, out var country))
            return;
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
        { MessageBox.ErrorQuery("Validation", "City and Country are required.", "OK"); return; }

        DateTime? start = DialogHelpers.PromptDate("Add Stay", "Start date (optional, Enter to skip)", "");
        DateTime? end   = start.HasValue ? DialogHelpers.PromptDate("Add Stay", "End date", "") : null;

        try { _svc.AddStay(city, country, start, end); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnDeleteStay()
    {
        var s = Selected;
        if (s is null) { MessageBox.ErrorQuery("", "Select a stay first.", "OK"); return; }
        if (!DialogHelpers.Confirm("Delete Stay", $"Delete '{s.DisplayKey}'?")) return;
        try { _svc.DeleteStay(s.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnArchive()
    {
        if (!DialogHelpers.Confirm("Archive Trip", "Archive this trip? It will be hidden from the trips list.")) return;
        try { _svc.ArchiveActiveTrip(); Application.RequestStop(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }
}
