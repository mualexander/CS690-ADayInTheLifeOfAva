using Terminal.Gui;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

/// <summary>Shows stay details and provides navigation to expenses/bookmarks/flights/lodging.</summary>
public class StayView : Window
{
    private readonly TripService _svc;
    private readonly InMemoryTripContext _ctx;
    private StaySummary _stay;

    private readonly Label _lCity, _lCountry, _lDates, _lTotal, _lExp, _lFly, _lLodge;

    public StayView(TripService svc, InMemoryTripContext ctx, StaySummary stay) : base()
    {
        Title = "Stay";
        _svc = svc;
        _ctx = ctx;
        _stay = stay;
        X = 0; Y = 0; Width = Dim.Fill(); Height = Dim.Fill();

        // Stay details frame
        var infoFrame = new FrameView { Title = "Stay Details", X = 0, Y = 0, Width = Dim.Fill(), Height = 9 };
        _lCity    = new Label { Text = "", X = 1, Y = 0 };
        _lCountry = new Label { Text = "", X = 1, Y = 1 };
        _lDates   = new Label { Text = "", X = 1, Y = 2 };
        _lTotal   = new Label { Text = "", X = 1, Y = 4 };
        _lExp     = new Label { Text = "", X = 1, Y = 5 };
        _lFly     = new Label { Text = "", X = 1, Y = 6 };
        _lLodge   = new Label { Text = "", X = 1, Y = 7 };
        infoFrame.Add(_lCity, _lCountry, _lDates, _lTotal, _lExp, _lFly, _lLodge);
        Add(infoFrame);

        // Sub-section navigation
        var navY = Pos.Bottom(infoFrame) + 1;
        var lblNav       = new Label { Text = "Navigate to:", X = 1,                          Y = navY };
        var btnExpenses  = new Button { Text = "_Expenses",   X = 1,                          Y = navY + 1 };
        var btnBookmarks = new Button { Text = "_Bookmarks",  X = Pos.Right(btnExpenses)  + 1, Y = navY + 1 };
        var btnFlights   = new Button { Text = "_Flights",    X = Pos.Right(btnBookmarks) + 1, Y = navY + 1 };
        var btnLodging   = new Button { Text = "_Lodging",    X = Pos.Right(btnFlights)   + 1, Y = navY + 1 };

        btnExpenses.Accepting  += (_, _) => { Application.Run(new ExpensesView(_svc, _stay));       RefreshStay(); };
        btnBookmarks.Accepting += (_, _) => { Application.Run(new BookmarksView(_svc, _stay));      RefreshStay(); };
        btnFlights.Accepting   += (_, _) => { Application.Run(new FlightOptionsView(_svc, _stay));  RefreshStay(); };
        btnLodging.Accepting   += (_, _) => { Application.Run(new LodgingOptionsView(_svc, _stay)); RefreshStay(); };

        Add(lblNav, btnExpenses, btnBookmarks, btnFlights, btnLodging);

        // Edit / action buttons
        var editY    = Pos.AnchorEnd(3);
        var lblEdit  = new Label { Text = "Edit stay:", X = 1, Y = editY };
        var btnPlace   = new Button { Text = "Set _Place",       X = 1,                       Y = editY + 1 };
        var btnStart   = new Button { Text = "Set S_tart Date",  X = Pos.Right(btnPlace)  + 1, Y = editY + 1 };
        var btnEnd     = new Button { Text = "Set _End Date",    X = Pos.Right(btnStart)  + 1, Y = editY + 1 };
        var btnDelStay = new Button { Text = "_Delete Stay",     X = Pos.Right(btnEnd)    + 1, Y = editY + 1 };
        var btnBack    = new Button { Text = "(Esc) Back",        X = Pos.AnchorEnd(14),        Y = editY + 1 };

        btnPlace.Accepting   += (_, _) => OnSetPlace();
        btnStart.Accepting   += (_, _) => OnSetStartDate();
        btnEnd.Accepting     += (_, _) => OnSetEndDate();
        btnDelStay.Accepting += (_, _) => OnDeleteStay();
        btnBack.Accepting    += (_, _) => Application.RequestStop();

        Add(lblEdit, btnPlace, btnStart, btnEnd, btnDelStay, btnBack);

        RefreshInfo();
        Application.KeyDown += OnAppKeyDown;
        Closed += (_, _) => Application.KeyDown -= OnAppKeyDown;
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
                    case 'e': Application.Run(new ExpensesView(_svc, _stay));       RefreshStay(); key.Handled = true; return;
                    case 'b': Application.Run(new BookmarksView(_svc, _stay));      RefreshStay(); key.Handled = true; return;
                    case 'f': Application.Run(new FlightOptionsView(_svc, _stay));  RefreshStay(); key.Handled = true; return;
                    case 'l': Application.Run(new LodgingOptionsView(_svc, _stay)); RefreshStay(); key.Handled = true; return;
                    case 'p': OnSetPlace();    key.Handled = true; return;
                    case 'd': OnDeleteStay();  key.Handled = true; return;
                }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void RefreshStay()
    {
        var updated = _svc.GetStays().FirstOrDefault(s => s.Id == _stay.Id);
        if (updated is not null) _stay = updated;
        RefreshInfo();
    }

    private void RefreshInfo()
    {
        Title         = $"Stay: {_stay.DisplayKey}";
        _lCity.Text   = $"City:     {_stay.City}";
        _lCountry.Text = $"Country:  {_stay.Country}";
        _lDates.Text  = _stay.StartDate.HasValue
            ? $"Dates:    {_stay.StartDate:yyyy-MM-dd} → {_stay.EndDate:yyyy-MM-dd}"
            : "Dates:    (not set)";
        _lTotal.Text  = $"Total Cost:   {_stay.TotalPlannedCost:0.00}";
        _lExp.Text    = $"  Expenses:   {_stay.ExpenseTotal:0.00}";
        _lFly.Text    = $"  Flights:    {_stay.SelectedFlightTotal:0.00}";
        _lLodge.Text  = $"  Lodging:    {_stay.SelectedLodgingTotal:0.00}";
        SetNeedsDraw();
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void OnSetPlace()
    {
        if (!DialogHelpers.PromptTwoFields("Set Place", "City:", _stay.City, "Country:", _stay.Country, out var city, out var country))
            return;
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
        { MessageBox.ErrorQuery("Validation", "City and Country are required.", "OK"); return; }
        try { _svc.UpdateStayPlace(_stay.Id, city, country); RefreshStay(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnSetStartDate()
    {
        var date = DialogHelpers.PromptDate("Set Start Date", "Start date", _stay.StartDate?.ToString("yyyy-MM-dd") ?? "");
        if (!date.HasValue) return;
        try { _svc.UpdateStayStartDate(_stay.Id, date.Value); RefreshStay(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnSetEndDate()
    {
        var date = DialogHelpers.PromptDate("Set End Date", "End date", _stay.EndDate?.ToString("yyyy-MM-dd") ?? "");
        if (!date.HasValue) return;
        try { _svc.UpdateStayEndDate(_stay.Id, date.Value); RefreshStay(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnDeleteStay()
    {
        if (!DialogHelpers.Confirm("Delete Stay", $"Delete '{_stay.DisplayKey}'? This cannot be undone.")) return;
        try { _svc.DeleteStay(_stay.Id); Application.RequestStop(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }
}
