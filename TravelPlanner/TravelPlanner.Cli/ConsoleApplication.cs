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
    private FlightOptionSummary? _activeFlightOption;

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
                            _activeFlightOption = null;
                        }
                        break;

                    case AppMode.TripMenu:
                        _mode = TripMenuFlow.Handle(_svc, _ctx, ref _activeStay);

                        if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        break;

                    case AppMode.StayMenu:
                        _mode = StayMenuFlow.Handle(_svc, ref _activeStay);

                        if (_mode == AppMode.TripMenu)
                        {
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        break;

                    case AppMode.ExpenseMenu:
                        _mode = ExpenseMenuFlow.Handle(_svc, _activeStay, ref _activeExpense);

                        if (_mode == AppMode.StayMenu)
                        {
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        break;

                    case AppMode.ExpenseDetailMenu:
                        _mode = ExpenseDetailMenuFlow.Handle(_svc, _activeStay, ref _activeExpense);

                        if (_mode == AppMode.ExpenseMenu)
                        {
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.StayMenu)
                        {
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeExpense = null;
                            _activeBookmark = null;
                            _activeFlightOption = null;
                        }
                        break;

                    case AppMode.BookmarkMenu:
                        _mode = BookmarkMenuFlow.Handle(_svc, _activeStay, ref _activeBookmark);

                        if (_mode == AppMode.StayMenu)
                        {
                            _activeBookmark = null;
                            _activeExpense = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeBookmark = null;
                            _activeExpense = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeBookmark = null;
                            _activeExpense = null;
                            _activeFlightOption = null;
                        }
                        break;

                    case AppMode.BookmarkDetailMenu:
                        _mode = BookmarkDetailMenuFlow.Handle(_svc, _activeStay, ref _activeBookmark);

                        if (_mode == AppMode.BookmarkMenu)
                        {
                            _activeBookmark = null;
                            _activeExpense = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.StayMenu)
                        {
                            _activeBookmark = null;
                            _activeExpense = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeBookmark = null;
                            _activeExpense = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeBookmark = null;
                            _activeExpense = null;
                            _activeFlightOption = null;
                        }
                        break;

                    case AppMode.FlightOptionMenu:
                        _mode = FlightOptionMenuFlow.Handle(_svc, _activeStay, ref _activeFlightOption);

                        if (_mode == AppMode.StayMenu)
                        {
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeFlightOption = null;
                        }
                        break;

                    case AppMode.FlightOptionDetailMenu:
                        _mode = FlightOptionDetailMenuFlow.Handle(_svc, _activeStay, ref _activeFlightOption);

                        if (_mode == AppMode.FlightOptionMenu)
                        {
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.StayMenu)
                        {
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.TripMenu)
                        {
                            _activeStay = null;
                            _activeFlightOption = null;
                        }
                        else if (_mode == AppMode.MainMenu)
                        {
                            _activeStay = null;
                            _activeFlightOption = null;
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