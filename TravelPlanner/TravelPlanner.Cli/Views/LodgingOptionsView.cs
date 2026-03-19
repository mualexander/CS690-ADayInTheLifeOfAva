using System.Collections.ObjectModel;
using Terminal.Gui;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

/// <summary>Lodging option management for a stay.</summary>
public class LodgingOptionsView : Window
{
    private readonly TripService _svc;
    private StaySummary _stay;

    private readonly ListView _listView;
    private readonly Label _lProperty, _lCheckIn, _lCheckOut, _lPrice, _lUrl, _lSelected, _lCreated;
    private List<LodgingOptionSummary> _options = new();
    private readonly ObservableCollection<string> _listSource = new();

    public LodgingOptionsView(TripService svc, StaySummary stay) : base()
    {
        Title = "Lodging Options";
        _svc  = svc;
        _stay = stay;
        X = 0; Y = 0; Width = Dim.Fill(); Height = Dim.Fill();

        // Left — options list
        var leftFrame = new FrameView { Title = "Lodging Options", X = 0, Y = 0, Width = Dim.Percent(50), Height = Dim.Fill(3) };
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
        _lProperty  = new Label { Text = "", X = 1, Y = 0 };
        _lCheckIn   = new Label { Text = "", X = 1, Y = 1 };
        _lCheckOut  = new Label { Text = "", X = 1, Y = 2 };
        _lPrice     = new Label { Text = "", X = 1, Y = 3 };
        _lUrl       = new Label { Text = "", X = 1, Y = 4 };
        _lSelected  = new Label { Text = "", X = 1, Y = 5 };
        _lCreated   = new Label { Text = "", X = 1, Y = 7 };
        rightFrame.Add(_lProperty, _lCheckIn, _lCheckOut, _lPrice, _lUrl, _lSelected, _lCreated);
        Add(rightFrame);

        // Buttons
        var y = Pos.AnchorEnd(3);
        var btnAdd    = new Button { Text = "_Add",      X = 1,                       Y = y };
        var btnPrice  = new Button { Text = "_Price",    X = Pos.Right(btnAdd)    + 1, Y = y };
        var btnUrl    = new Button { Text = "_URL",      X = Pos.Right(btnPrice)  + 1, Y = y };
        var btnSel    = new Button { Text = "_Select",   X = Pos.Right(btnUrl)    + 1, Y = y };
        var btnDesel  = new Button { Text = "D_eselect", X = Pos.Right(btnSel)    + 1, Y = y };
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

    private LodgingOptionSummary? Selected =>
        _options.Count > 0 && _listView.SelectedItem >= 0 && _listView.SelectedItem < _options.Count
            ? _options[_listView.SelectedItem]
            : null;

    private void Refresh()
    {
        Title = $"Lodging — {_stay.DisplayKey}";
        _options = _svc.GetLodgingOptionsForStay(_stay.Id).ToList();
        _listSource.Clear();
        foreach (var l in _options)
            _listSource.Add($" {l.PropertyName}  {l.CheckInDate:MM-dd}→{l.CheckOutDate:MM-dd}{(l.IsSelected ? "  ✓" : "")}");
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        var l = Selected;
        if (l is null)
        {
            _lProperty.Text = "Select a lodging option to view details.";
            _lCheckIn.Text  = _lCheckOut.Text = _lPrice.Text = "";
            _lUrl.Text      = _lSelected.Text = _lCreated.Text = "";
            return;
        }
        _lProperty.Text = $"Property: {l.PropertyName}";
        _lCheckIn.Text  = $"Check-in:  {l.CheckInDate:yyyy-MM-dd}";
        _lCheckOut.Text = $"Check-out: {l.CheckOutDate:yyyy-MM-dd}";
        _lPrice.Text    = $"Price:     {(l.Price.HasValue ? l.Price.Value.ToString("0.00") : "(unknown)")}";
        _lUrl.Text      = $"URL:       {l.Url}";
        _lSelected.Text = $"Selected:  {(l.IsSelected ? "Yes ✓" : "No")}";
        _lCreated.Text  = $"Created:   {l.CreatedAt:yyyy-MM-dd HH:mm} UTC";
        SetNeedsDraw();
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void OnAdd()
    {
        var url = DialogHelpers.PromptText("Add Lodging Option", "URL:", "");
        if (string.IsNullOrWhiteSpace(url)) return;

        var name = DialogHelpers.PromptText("Add Lodging Option", "Property name:", "");
        if (string.IsNullOrWhiteSpace(name)) return;

        var checkIn = DialogHelpers.PromptDate("Add Lodging Option", "Check-in date", "");
        if (!checkIn.HasValue) return;

        var checkOut = DialogHelpers.PromptDate("Add Lodging Option", "Check-out date", "");
        if (!checkOut.HasValue) return;

        var price = DialogHelpers.PromptOptionalDecimal("Add Lodging Option", "Price", "");

        try
        {
            _svc.AddLodgingOptionToStay(_stay.Id, url, name, checkIn.Value, checkOut.Value, price);
            Refresh();
        }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnUpdatePrice()
    {
        var l = Selected;
        if (l is null) { MessageBox.ErrorQuery("", "Select a lodging option first.", "OK"); return; }
        var price = DialogHelpers.PromptOptionalDecimal("Update Price", "Price", l.Price?.ToString("0.00") ?? "");
        try { _svc.UpdateLodgingOptionPrice(_stay.Id, l.Id, price); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnUpdateUrl()
    {
        var l = Selected;
        if (l is null) { MessageBox.ErrorQuery("", "Select a lodging option first.", "OK"); return; }
        var newUrl = DialogHelpers.PromptText("Update URL", "New URL:", l.Url);
        if (string.IsNullOrWhiteSpace(newUrl)) return;
        try { _svc.UpdateLodgingOptionUrl(_stay.Id, l.Id, newUrl); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnMarkSelected()
    {
        var l = Selected;
        if (l is null) { MessageBox.ErrorQuery("", "Select a lodging option first.", "OK"); return; }
        try { _svc.SelectLodgingOption(_stay.Id, l.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnMarkDeselected()
    {
        var l = Selected;
        if (l is null) { MessageBox.ErrorQuery("", "Select a lodging option first.", "OK"); return; }
        try { _svc.DeselectLodgingOption(_stay.Id, l.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnDelete()
    {
        var l = Selected;
        if (l is null) { MessageBox.ErrorQuery("", "Select a lodging option first.", "OK"); return; }
        if (!DialogHelpers.Confirm("Delete Lodging Option", $"Delete '{l.PropertyName}'?")) return;
        try { _svc.DeleteLodgingOption(_stay.Id, l.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }
}
