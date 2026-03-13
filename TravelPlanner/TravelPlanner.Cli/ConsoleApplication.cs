using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public class ConsoleApplication
{
    private readonly TripService _svc;
    private readonly InMemoryTripContext _ctx;
    private readonly string _dataPath;

    private AppMode _mode = AppMode.MainMenu;
    private StaySummary? _activeStay;
    private ExpenseSummary? _activeExpense;
    private BookmarkSummary? _activeBookmark;

    public ConsoleApplication(TripService svc, InMemoryTripContext ctx, string dataPath)
    {
        _svc = svc;
        _ctx = ctx;
        _dataPath = dataPath;
    }

    public void Run()
    {
        MenuRenderer.ShowMessage($"TravelPlanner (data: {_dataPath})");
        MenuRenderer.BlankLine();

        while (true)
        {
            try
            {
                MenuRenderer.ShowHeader(_ctx.ActiveTrip, _activeStay);

                switch (_mode)
                {
                    case AppMode.MainMenu:
                        _mode = MainMenuFlow.Handle(_svc, _ctx);

                        if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                        }
                        break;

                    case AppMode.TripMenu:
                        _mode = TripMenuFlow.Handle(_svc, _ctx, ref _activeStay);

                        if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                        }
                        break;

                    case AppMode.StayMenu:
                        _mode = StayMenuFlow.Handle(_svc, ref _activeStay);

                        if (_mode == AppMode.TripMenu)
                        {
                            _activeExpense = null;
                            _activeBookmark = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                        }
                        break;

                    case AppMode.ExpenseMenu:
                        _mode = ExpenseMenuFlow.Handle(_svc, _activeStay, ref _activeExpense);

                        if (_mode == AppMode.StayMenu)
                        {
                            _activeExpense = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                        }
                        break;

                    case AppMode.ExpenseDetailMenu:
                        _mode = ExpenseDetailMenuFlow.Handle(_svc, _activeStay, ref _activeExpense);

                        if (_mode == AppMode.ExpenseMenu)
                        {
                            _activeExpense = null;
                        }
                        else if (_mode == AppMode.StayMenu)
                        {
                            _activeExpense = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                        }
                        break;

                    case AppMode.BookmarkMenu:
                        _mode = BookmarkMenuFlow.Handle(_svc, _activeStay, ref _activeBookmark);

                        if (_mode == AppMode.StayMenu)
                        {
                            _activeBookmark = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeBookmark = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeBookmark = null;
                        }
                        break;

                    case AppMode.BookmarkDetailMenu:
                        _mode = BookmarkDetailMenuFlow.Handle(_svc, _activeStay, ref _activeBookmark);

                        if (_mode == AppMode.BookmarkMenu)
                        {
                            _activeBookmark = null;
                        }
                        else if (_mode == AppMode.StayMenu)
                        {
                            _activeBookmark = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeBookmark = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeBookmark = null;
                        }
                        break;

                    default:
                        MenuRenderer.ShowError("Unknown application mode.");
                        _mode = AppMode.MainMenu;
                        _activeStay = null;
                        _activeExpense = null;
                        _activeBookmark = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                MenuRenderer.ShowError(ex.Message);
            }

            MenuRenderer.BlankLine();
        }
    }
}