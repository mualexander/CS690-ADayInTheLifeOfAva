using System.Collections.ObjectModel;
using Terminal.Gui;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

/// <summary>Flight option management for a stay.</summary>
public class FlightOptionsView : Window
{
    private readonly TripService _svc;
    private StaySummary _stay;

    private readonly ListView _listView;
    private readonly Label _lRoute, _lDepart, _lArrive, _lPrice, _lUrl, _lSelected, _lCreated;
    private List<FlightOptionSummary> _options = new();
    private readonly ObservableCollection<string> _listSource = new();

    public FlightOptionsView(TripService svc, StaySummary stay) : base()
    {
        Title = "Flight Options";
        _svc  = svc;
        _stay = stay;
        X = 0; Y = 0; Width = Dim.Fill(); Height = Dim.Fill();

        // Left — options list
        var leftFrame = new FrameView { Title = "Flight Options", X = 0, Y = 0, Width = Dim.Percent(50), Height = Dim.Fill(3) };
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

        // Right — detail
        var rightFrame = new FrameView { Title = "Details", X = Pos.Right(leftFrame), Y = 0, Width = Dim.Fill(), Height = Dim.Fill(3) };
        _lRoute    = new Label { Text = "", X = 1, Y = 0 };
        _lDepart   = new Label { Text = "", X = 1, Y = 1 };
        _lArrive   = new Label { Text = "", X = 1, Y = 2 };
        _lPrice    = new Label { Text = "", X = 1, Y = 3 };
        _lUrl      = new Label { Text = "", X = 1, Y = 4 };
        _lSelected = new Label { Text = "", X = 1, Y = 5 };
        _lCreated  = new Label { Text = "", X = 1, Y = 7 };
        rightFrame.Add(_lRoute, _lDepart, _lArrive, _lPrice, _lUrl, _lSelected, _lCreated);
        Add(rightFrame);

        // Buttons
        var y = Pos.AnchorEnd(3);
        var btnAdd    = new Button { Text = "_Add",      X = 1,                       Y = y };
        var btnPrice  = new Button { Text = "_Price",    X = Pos.Right(btnAdd)    + 1, Y = y };
        var btnUrl    = new Button { Text = "_URL",      X = Pos.Right(btnPrice)  + 1, Y = y };
        var btnSel    = new Button { Text = "_Select",   X = Pos.Right(btnUrl)    + 1, Y = y };
        var btnDesel  = new Button { Text = "_Deselect", X = Pos.Right(btnSel)    + 1, Y = y };
        var btnDelete = new Button { Text = "De_lete",   X = Pos.Right(btnDesel)  + 1, Y = y };
        var btnBack   = new Button { Text = "(Esc) Back", X = Pos.AnchorEnd(14),       Y = y };

        btnAdd.Accepting    += (_, _) => OnAdd();
        btnPrice.Accepting  += (_, _) => OnUpdatePrice();
        btnUrl.Accepting    += (_, _) => OnUpdateUrl();
        btnSel.Accepting    += (_, _) => OnMarkSelected();
        btnDesel.Accepting  += (_, _) => OnMarkDeselected();
        btnDelete.Accepting += (_, _) => OnDelete();
        btnBack.Accepting   += (_, _) => Application.RequestStop();

        Add(btnAdd, btnPrice, btnUrl, btnSel, btnDesel, btnDelete, btnBack);

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
                    case 'a': OnAdd();            key.Handled = true; return;
                    case 'p': OnUpdatePrice();    key.Handled = true; return;
                    case 'u': OnUpdateUrl();      key.Handled = true; return;
                    case 's': OnMarkSelected();   key.Handled = true; return;
                    case 'd': OnMarkDeselected(); key.Handled = true; return;
                }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private FlightOptionSummary? Selected =>
        _options.Count > 0 && _listView.SelectedItem >= 0 && _listView.SelectedItem < _options.Count
            ? _options[_listView.SelectedItem]
            : null;

    private void Refresh()
    {
        Title = $"Flights — {_stay.DisplayKey}";
        _options = _svc.GetFlightOptionsForStay(_stay.Id).ToList();
        _listSource.Clear();
        foreach (var f in _options)
            _listSource.Add($" {f.FromAirportCode}→{f.ToAirportCode}  {f.DepartTime:MM-dd HH:mm}{(f.IsSelected ? "  ✓" : "")}");
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        var f = Selected;
        if (f is null)
        {
            _lRoute.Text    = "Select a flight option to view details.";
            _lDepart.Text   = _lArrive.Text = _lPrice.Text = "";
            _lUrl.Text      = _lSelected.Text = _lCreated.Text = "";
            return;
        }
        _lRoute.Text    = $"Route:    {f.FromAirportCode} → {f.ToAirportCode}";
        _lDepart.Text   = $"Depart:   {f.DepartTime:yyyy-MM-dd HH:mm}";
        _lArrive.Text   = $"Arrive:   {f.ArriveTime:yyyy-MM-dd HH:mm}";
        _lPrice.Text    = $"Price:    {(f.Price.HasValue ? f.Price.Value.ToString("0.00") : "(unknown)")}";
        _lUrl.Text      = $"URL:      {f.Url}";
        _lSelected.Text = $"Selected: {(f.IsSelected ? "Yes ✓" : "No")}";
        _lCreated.Text  = $"Created:  {f.CreatedAt:yyyy-MM-dd HH:mm} UTC";
        SetNeedsDraw();
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void OnAdd()
    {
        var url = DialogHelpers.PromptText("Add Flight Option", "URL:", "");
        if (string.IsNullOrWhiteSpace(url)) return;

        var from = DialogHelpers.PromptText("Add Flight Option", "From airport (e.g. JFK):", "");
        if (string.IsNullOrWhiteSpace(from)) return;

        var to = DialogHelpers.PromptText("Add Flight Option", "To airport (e.g. NRT):", "");
        if (string.IsNullOrWhiteSpace(to)) return;

        var depart = DialogHelpers.PromptDateTime("Add Flight Option", "Depart time", "");
        if (!depart.HasValue) return;

        var arrive = DialogHelpers.PromptDateTime("Add Flight Option", "Arrive time", "");
        if (!arrive.HasValue) return;

        var price = DialogHelpers.PromptOptionalDecimal("Add Flight Option", "Price", "");

        try
        {
            _svc.AddFlightOptionToStay(_stay.Id, url, from, to, depart.Value, arrive.Value, price);
            Refresh();
        }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnUpdatePrice()
    {
        var f = Selected;
        if (f is null) { MessageBox.ErrorQuery("", "Select a flight option first.", "OK"); return; }
        var price = DialogHelpers.PromptOptionalDecimal("Update Price", "Price", f.Price?.ToString("0.00") ?? "");
        try { _svc.UpdateFlightOptionPrice(_stay.Id, f.Id, price); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnUpdateUrl()
    {
        var f = Selected;
        if (f is null) { MessageBox.ErrorQuery("", "Select a flight option first.", "OK"); return; }
        var newUrl = DialogHelpers.PromptText("Update URL", "New URL:", f.Url);
        if (string.IsNullOrWhiteSpace(newUrl)) return;
        try { _svc.UpdateFlightOptionUrl(_stay.Id, f.Id, newUrl); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnMarkSelected()
    {
        var f = Selected;
        if (f is null) { MessageBox.ErrorQuery("", "Select a flight option first.", "OK"); return; }
        try { _svc.SelectFlightOption(_stay.Id, f.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnMarkDeselected()
    {
        var f = Selected;
        if (f is null) { MessageBox.ErrorQuery("", "Select a flight option first.", "OK"); return; }
        try { _svc.DeselectFlightOption(_stay.Id, f.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnDelete()
    {
        var f = Selected;
        if (f is null) { MessageBox.ErrorQuery("", "Select a flight option first.", "OK"); return; }
        if (!DialogHelpers.Confirm("Delete Flight Option", $"Delete {f.FromAirportCode}→{f.ToAirportCode}?")) return;
        try { _svc.DeleteFlightOption(_stay.Id, f.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }
}
