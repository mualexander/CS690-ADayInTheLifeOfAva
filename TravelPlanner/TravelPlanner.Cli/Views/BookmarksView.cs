using System.Collections.ObjectModel;
using Terminal.Gui;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli.Views;

/// <summary>Bookmark management for a stay.</summary>
public class BookmarksView : Window
{
    private readonly TripService _svc;
    private StaySummary _stay;

    private readonly ListView _listView;
    private readonly Label _lTitle, _lUrl, _lNotes, _lCreated;
    private List<BookmarkSummary> _bookmarks = new();
    private readonly ObservableCollection<string> _listSource = new();

    public BookmarksView(TripService svc, StaySummary stay) : base()
    {
        Title = "Bookmarks";
        _svc  = svc;
        _stay = stay;
        X = 0; Y = 0; Width = Dim.Fill(); Height = Dim.Fill();

        // Left — bookmark list
        var leftFrame = new FrameView { Title = "Bookmarks", X = 0, Y = 0, Width = Dim.Percent(50), Height = Dim.Fill(3) };
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

        // Right — bookmark detail
        var rightFrame = new FrameView { Title = "Details", X = Pos.Right(leftFrame), Y = 0, Width = Dim.Fill(), Height = Dim.Fill(3) };
        _lTitle   = new Label { Text = "", X = 1, Y = 0 };
        _lUrl     = new Label { Text = "", X = 1, Y = 1 };
        _lNotes   = new Label { Text = "", X = 1, Y = 2 };
        _lCreated = new Label { Text = "", X = 1, Y = 4 };
        rightFrame.Add(_lTitle, _lUrl, _lNotes, _lCreated);
        Add(rightFrame);

        // Buttons
        var y = Pos.AnchorEnd(2);
        var btnAdd    = new Button { Text = "_Add",    X = 1,                       Y = y };
        var btnRename = new Button { Text = "_Rename", X = Pos.Right(btnAdd)    + 1, Y = y };
        var btnUrl    = new Button { Text = "_URL",    X = Pos.Right(btnRename) + 1, Y = y };
        var btnNotes  = new Button { Text = "_Notes",  X = Pos.Right(btnUrl)    + 1, Y = y };
        var btnDelete = new Button { Text = "_Delete", X = Pos.Right(btnNotes)  + 1, Y = y };
        var btnBack   = new Button { Text = "(Esc) Back", X = Pos.AnchorEnd(14),      Y = y };

        btnAdd.Accepting    += (_, _) => OnAdd();
        btnRename.Accepting += (_, _) => OnRename();
        btnUrl.Accepting    += (_, _) => OnUpdateUrl();
        btnNotes.Accepting  += (_, _) => OnUpdateNotes();
        btnDelete.Accepting += (_, _) => OnDelete();
        btnBack.Accepting   += (_, _) => Application.RequestStop();

        Add(btnAdd, btnRename, btnUrl, btnNotes, btnDelete, btnBack);

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
                    case 'u': OnUpdateUrl();     key.Handled = true; return;
                    case 'n': OnUpdateNotes();   key.Handled = true; return;
                    case 'd': OnDelete();        key.Handled = true; return;
                }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private BookmarkSummary? Selected =>
        _bookmarks.Count > 0 && _listView.SelectedItem >= 0 && _listView.SelectedItem < _bookmarks.Count
            ? _bookmarks[_listView.SelectedItem]
            : null;

    private void Refresh()
    {
        Title = $"Bookmarks — {_stay.DisplayKey}";
        _bookmarks = _svc.GetBookmarksForStay(_stay.Id).ToList();
        _listSource.Clear();
        foreach (var b in _bookmarks)
            _listSource.Add($" {b.Title}");
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        var b = Selected;
        if (b is null)
        {
            _lTitle.Text = "Select a bookmark to view details.";
            _lUrl.Text   = _lNotes.Text = _lCreated.Text = "";
            return;
        }
        _lTitle.Text   = $"Title:   {b.Title}";
        _lUrl.Text     = $"URL:     {b.Url}";
        _lNotes.Text   = $"Notes:   {b.Notes ?? "(none)"}";
        _lCreated.Text = $"Created: {b.CreatedAt:yyyy-MM-dd HH:mm} UTC";
        SetNeedsDraw();
    }

    // ── Handlers ─────────────────────────────────────────────────────────────

    private void OnAdd()
    {
        var title = DialogHelpers.PromptText("Add Bookmark", "Title:", "");
        if (string.IsNullOrWhiteSpace(title)) return;
        var url = DialogHelpers.PromptText("Add Bookmark", "URL:", "");
        if (string.IsNullOrWhiteSpace(url)) return;
        var notes = DialogHelpers.PromptText("Add Bookmark", "Notes (optional):", "");
        try
        {
            _svc.AddBookmarkToStay(_stay.Id, title, url, string.IsNullOrWhiteSpace(notes) ? null : notes);
            Refresh();
        }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnRename()
    {
        var b = Selected;
        if (b is null) { MessageBox.ErrorQuery("", "Select a bookmark first.", "OK"); return; }
        var newTitle = DialogHelpers.PromptText("Rename Bookmark", "New title:", b.Title);
        if (string.IsNullOrWhiteSpace(newTitle)) return;
        try { _svc.UpdateBookmarkTitle(_stay.Id, b.Id, newTitle); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnUpdateUrl()
    {
        var b = Selected;
        if (b is null) { MessageBox.ErrorQuery("", "Select a bookmark first.", "OK"); return; }
        var newUrl = DialogHelpers.PromptText("Update URL", "New URL:", b.Url);
        if (string.IsNullOrWhiteSpace(newUrl)) return;
        try { _svc.UpdateBookmarkUrl(_stay.Id, b.Id, newUrl); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnUpdateNotes()
    {
        var b = Selected;
        if (b is null) { MessageBox.ErrorQuery("", "Select a bookmark first.", "OK"); return; }
        var notes = DialogHelpers.PromptText("Update Notes", "Notes (blank clears):", b.Notes ?? "");
        if (notes == null) return;
        try { _svc.UpdateBookmarkNotes(_stay.Id, b.Id, string.IsNullOrWhiteSpace(notes) ? null : notes); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }

    private void OnDelete()
    {
        var b = Selected;
        if (b is null) { MessageBox.ErrorQuery("", "Select a bookmark first.", "OK"); return; }
        if (!DialogHelpers.Confirm("Delete Bookmark", $"Delete '{b.Title}'?")) return;
        try { _svc.DeleteBookmark(_stay.Id, b.Id); Refresh(); }
        catch (Exception ex) { MessageBox.ErrorQuery("Error", ex.Message, "OK"); }
    }
}
