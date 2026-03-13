using System;
using System.Globalization;
using System.Linq;
using TravelPlanner.Console.Commands;
using TravelPlanner.Console.Parsers;
using TravelPlanner.ConsoleApp;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;

const string DataPath = "data/trips.json";

var repo = new FileTripRepository(DataPath);
var ctx = new InMemoryTripContext(repo);
var svc = new TripService(repo, ctx);

var mode = AppMode.MainMenu;
StaySummary? activeStay = null;
ExpenseSummary? activeExpense = null;
BookmarkSummary? activeBookmark = null;

MenuRenderer.ShowMessage($"TravelPlanner (data: {DataPath})");
MenuRenderer.BlankLine();

while (true)
{
    try
    {
        MenuRenderer.ShowHeader(ctx.ActiveTrip, activeStay);

        switch (mode)
        {
            case AppMode.MainMenu:
                mode = HandleMainMenu(svc, ctx);

                if (mode == AppMode.MainMenu)
                {
                    activeStay = null;
                    activeBookmark = null;
                }
                break;

            case AppMode.TripMenu:
                mode = HandleTripMenu(svc, ctx, ref activeStay);

                if (mode == AppMode.MainMenu)
                {
                    activeStay = null;
                    activeBookmark = null;
                }
                break;

            case AppMode.StayMenu:
                mode = HandleStayMenu(svc, ref activeStay);

                if (mode == AppMode.TripMenu)
                {
                    activeBookmark = null;
                }
                else if (mode == AppMode.MainMenu)
                {
                    activeStay = null;
                    activeBookmark = null;
                }
                break;

            case AppMode.ExpenseMenu:
                mode = HandleExpenseMenu(svc, activeStay, ref activeExpense);

                if (mode == AppMode.StayMenu)
                {
                    activeExpense = null;
                }
                else if (mode == AppMode.TripMenu)
                {
                    activeStay = null;
                    activeExpense = null;
                }
                else if (mode == AppMode.MainMenu)
                {
                    activeStay = null;
                    activeExpense = null;
                }
                break;

            case AppMode.ExpenseDetailMenu:
                mode = HandleExpenseDetailMenu(svc, activeStay, ref activeExpense);

                if (mode == AppMode.ExpenseMenu)
                {
                    // keep activeStay, clear expense selection
                    activeExpense = null;
                }
                else if (mode == AppMode.StayMenu)
                {
                    activeExpense = null;
                }
                else if (mode == AppMode.TripMenu)
                {
                    activeStay = null;
                    activeExpense = null;
                }
                else if (mode == AppMode.MainMenu)
                {
                    activeStay = null;
                    activeExpense = null;
                }
                break;


            case AppMode.BookmarkMenu:
                mode = HandleBookmarkMenu(svc, activeStay, ref activeBookmark);

                if (mode == AppMode.StayMenu)
                {
                    activeBookmark = null;
                }
                else if (mode == AppMode.TripMenu)
                {
                    activeStay = null;
                    activeBookmark = null;
                }
                else if (mode == AppMode.MainMenu)
                {
                    activeStay = null;
                    activeBookmark = null;
                }
                break;

            case AppMode.BookmarkDetailMenu:
                mode = HandleBookmarkDetailMenu(svc, activeStay, ref activeBookmark);

                if (mode == AppMode.BookmarkMenu)
                {
                    // keep activeStay, clear bookmark selection
                    activeBookmark = null;
                }
                else if (mode == AppMode.StayMenu)
                {
                    activeBookmark = null;
                }
                else if (mode == AppMode.TripMenu)
                {
                    activeStay = null;
                    activeBookmark = null;
                }
                else if (mode == AppMode.MainMenu)
                {
                    activeStay = null;
                    activeBookmark = null;
                }
                break;

            default:
                MenuRenderer.ShowError("Unknown application mode.");
                mode = AppMode.MainMenu;
                activeStay = null;
                activeBookmark = null;
                break;
        }
    }
    catch (Exception ex)
    {
        MenuRenderer.ShowError(ex.Message);
    }

    MenuRenderer.BlankLine();
}

static AppMode HandleMainMenu(TripService svc, InMemoryTripContext ctx)
{
    MenuRenderer.ShowMainMenu();
    var command = MainMenuParser.Parse(Console.ReadLine());
    MenuRenderer.BlankLine();

    switch (command)
    {
        case MainMenuCommand.ListTrips:
            MenuRenderer.ShowTrips(svc.GetTrips());
            return AppMode.MainMenu;

        case MainMenuCommand.CreateTrip:
            CreateTrip(svc);
            return AppMode.TripMenu;

        case MainMenuCommand.SelectTrip:
            SelectTrip(svc);
            return AppMode.TripMenu;

        case MainMenuCommand.SeedTestData:
            SeedDemoData(svc);
            return AppMode.TripMenu;

        case MainMenuCommand.Quit:
            MenuRenderer.ShowMessage("Bye.");
            Environment.Exit(0);
            return AppMode.MainMenu;

        case MainMenuCommand.Unknown:
        default:
            MenuRenderer.ShowMessage("Unknown choice.");
            return AppMode.MainMenu;
    }
}

static AppMode HandleTripMenu(TripService svc, InMemoryTripContext ctx, ref StaySummary? activeStay)
{
    if (ctx.ActiveTrip is null)
    {
        MenuRenderer.ShowMessage("No active trip selected.");
        return AppMode.MainMenu;
    }

    MenuRenderer.ShowTripMenu();
    var command = TripMenuParser.Parse(Console.ReadLine());
    MenuRenderer.BlankLine();

    switch (command)
    {
        case TripMenuCommand.ListStays:
            MenuRenderer.ShowStays(svc.GetStays());
            return AppMode.TripMenu;

        case TripMenuCommand.AddStay:
            AddStay(svc);
            return AppMode.TripMenu;

        case TripMenuCommand.SelectStay:
            activeStay = SelectStay(svc);
            return activeStay is null ? AppMode.TripMenu : AppMode.StayMenu;

        case TripMenuCommand.Back:
            activeStay = null;
            return AppMode.MainMenu;

        case TripMenuCommand.Unknown:
        default:
            MenuRenderer.ShowMessage("Unknown choice.");
            return AppMode.TripMenu;
    }
}

static AppMode HandleStayMenu(TripService svc, ref StaySummary? activeStay)
{
    if (activeStay is null)
    {
        MenuRenderer.ShowMessage("No active stay selected.");
        return AppMode.TripMenu;
    }

    MenuRenderer.ShowStayMenu();
    var command = StayMenuParser.Parse(Console.ReadLine());
    MenuRenderer.BlankLine();

    switch (command)
    {
        case StayMenuCommand.ViewStayDetails:
            MenuRenderer.ShowStayDetails(activeStay);
            return AppMode.StayMenu;

        case StayMenuCommand.ManageExpenses:
            return AppMode.ExpenseMenu;

        case StayMenuCommand.ManageBookmarks:
            return AppMode.BookmarkMenu;

        case StayMenuCommand.SetPlace:
            SetStayPlace(svc, ref activeStay);
            return AppMode.StayMenu;

        case StayMenuCommand.SetStartDate:
            SetStayStartDate(svc, ref activeStay);
            return AppMode.StayMenu;

        case StayMenuCommand.SetEndDate:
            SetStayEndDate(svc, ref activeStay);
            return AppMode.StayMenu;

        case StayMenuCommand.DeleteStay:
            DeleteActiveStay(svc, ref activeStay);
            return AppMode.TripMenu;

        case StayMenuCommand.Back:
            activeStay = null;
            return AppMode.TripMenu;

        case StayMenuCommand.Unknown:
        default:
            MenuRenderer.ShowMessage("Unknown choice.");
            return AppMode.StayMenu;
    }
}

static AppMode HandleExpenseMenu(TripService svc, StaySummary? activeStay, ref ExpenseSummary? activeExpense)
{
    if (activeStay is null)
    {
        MenuRenderer.ShowMessage("No active stay selected.");
        return AppMode.TripMenu;
    }

    MenuRenderer.ShowExpenseMenu();
    var command = ExpenseMenuParser.Parse(Console.ReadLine());
    MenuRenderer.BlankLine();

    switch (command)
    {
        case ExpenseMenuCommand.ListExpenses:
            ListExpenses(svc, activeStay);
            return AppMode.ExpenseMenu;

        case ExpenseMenuCommand.AddExpense:
            AddExpense(svc, activeStay);
            return AppMode.ExpenseMenu;

        case ExpenseMenuCommand.SelectExpense:
            activeExpense = SelectExpense(svc, activeStay);
            return activeExpense is null ? AppMode.ExpenseMenu : AppMode.ExpenseDetailMenu;

        case ExpenseMenuCommand.DeleteExpense:
            DeleteExpense(svc, activeStay);
            activeExpense = null;
            return AppMode.ExpenseMenu;

        case ExpenseMenuCommand.Back:
            activeExpense = null;
            return AppMode.StayMenu;

        case ExpenseMenuCommand.Unknown:
        default:
            MenuRenderer.ShowMessage("Unknown choice.");
            return AppMode.ExpenseMenu;
    }
}

static AppMode HandleExpenseDetailMenu(
    TripService svc,
    StaySummary? activeStay,
    ref ExpenseSummary? activeExpense)
{
    if (activeStay is null)
    {
        MenuRenderer.ShowMessage("No active stay selected.");
        return AppMode.TripMenu;
    }

    if (activeExpense is null)
    {
        MenuRenderer.ShowMessage("No active Expense selected.");
        return AppMode.ExpenseMenu;
    }

    MenuRenderer.ShowExpenseDetailMenu();
    var command = ExpenseDetailMenuParser.Parse(Console.ReadLine());
    MenuRenderer.BlankLine();

    switch (command)
    {
        case ExpenseDetailMenuCommand.ViewDetails:
            MenuRenderer.ShowExpenseDetails(activeExpense);
            return AppMode.ExpenseDetailMenu;

        case ExpenseDetailMenuCommand.Rename:
            RenameExpense(svc, activeStay, ref activeExpense);
            return activeExpense is null ? AppMode.ExpenseMenu : AppMode.ExpenseDetailMenu;

        case ExpenseDetailMenuCommand.UpdateAmount:
            UpdateExpenseAmount(svc, activeStay, ref activeExpense);
            return activeExpense is null ? AppMode.ExpenseMenu : AppMode.ExpenseDetailMenu;

        case ExpenseDetailMenuCommand.UpdateNotes:
            UpdateExpenseNotes(svc, activeStay, ref activeExpense);
            return activeExpense is null ? AppMode.ExpenseMenu : AppMode.ExpenseDetailMenu;

        case ExpenseDetailMenuCommand.Delete:
            DeleteActiveExpense(svc, activeStay, ref activeExpense);
            return AppMode.ExpenseMenu;

        case ExpenseDetailMenuCommand.Back:
            activeExpense = null;
            return AppMode.ExpenseMenu;

        case ExpenseDetailMenuCommand.Unknown:
        default:
            MenuRenderer.ShowMessage("Unknown choice.");
            return AppMode.ExpenseDetailMenu;
    }
}


static AppMode HandleBookmarkMenu(TripService svc, StaySummary? activeStay, ref BookmarkSummary? activeBookmark)
{
    if (activeStay is null)
    {
        MenuRenderer.ShowMessage("No active stay selected.");
        return AppMode.TripMenu;
    }

    MenuRenderer.ShowBookmarkMenu();
    var command = BookmarkMenuParser.Parse(Console.ReadLine());
    MenuRenderer.BlankLine();

    switch (command)
    {
        case BookmarkMenuCommand.ListBookmarks:
            ListBookmarks(svc, activeStay);
            return AppMode.BookmarkMenu;

        case BookmarkMenuCommand.AddBookmark:
            AddBookmark(svc, activeStay);
            return AppMode.BookmarkMenu;

        case BookmarkMenuCommand.SelectBookmark:
            activeBookmark = SelectBookmark(svc, activeStay);
            return activeBookmark is null ? AppMode.BookmarkMenu : AppMode.BookmarkDetailMenu;

        case BookmarkMenuCommand.DeleteBookmark:
            DeleteBookmark(svc, activeStay);
            activeBookmark = null;
            return AppMode.BookmarkMenu;

        case BookmarkMenuCommand.Back:
            activeBookmark = null;
            return AppMode.StayMenu;

        case BookmarkMenuCommand.Unknown:
        default:
            MenuRenderer.ShowMessage("Unknown choice.");
            return AppMode.BookmarkMenu;
    }
}

static AppMode HandleBookmarkDetailMenu(
    TripService svc,
    StaySummary? activeStay,
    ref BookmarkSummary? activeBookmark)
{
    if (activeStay is null)
    {
        MenuRenderer.ShowMessage("No active stay selected.");
        return AppMode.TripMenu;
    }

    if (activeBookmark is null)
    {
        MenuRenderer.ShowMessage("No active bookmark selected.");
        return AppMode.BookmarkMenu;
    }

    MenuRenderer.ShowBookmarkDetailMenu();
    var command = BookmarkDetailMenuParser.Parse(Console.ReadLine());
    MenuRenderer.BlankLine();

    switch (command)
    {
        case BookmarkDetailMenuCommand.ViewDetails:
            MenuRenderer.ShowBookmarkDetails(activeBookmark);
            return AppMode.BookmarkDetailMenu;

        case BookmarkDetailMenuCommand.Rename:
            RenameBookmark(svc, activeStay, ref activeBookmark);
            return activeBookmark is null ? AppMode.BookmarkMenu : AppMode.BookmarkDetailMenu;

        case BookmarkDetailMenuCommand.UpdateUrl:
            UpdateBookmarkUrl(svc, activeStay, ref activeBookmark);
            return activeBookmark is null ? AppMode.BookmarkMenu : AppMode.BookmarkDetailMenu;

        case BookmarkDetailMenuCommand.UpdateNotes:
            UpdateBookmarkNotes(svc, activeStay, ref activeBookmark);
            return activeBookmark is null ? AppMode.BookmarkMenu : AppMode.BookmarkDetailMenu;

        case BookmarkDetailMenuCommand.Delete:
            DeleteActiveBookmark(svc, activeStay, ref activeBookmark);
            return AppMode.BookmarkMenu;

        case BookmarkDetailMenuCommand.Back:
            activeBookmark = null;
            return AppMode.BookmarkMenu;

        case BookmarkDetailMenuCommand.Unknown:
        default:
            MenuRenderer.ShowMessage("Unknown choice.");
            return AppMode.BookmarkDetailMenu;
    }
}

static void CreateTrip(TripService svc)
{
    Console.Write("Trip name: ");
    var name = (Console.ReadLine() ?? "").Trim();

    Console.Write("Total budget (e.g. 5000): ");
    var budgetStr = (Console.ReadLine() ?? "").Trim();

    if (!decimal.TryParse(budgetStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var budget))
        throw new ArgumentException("Invalid budget.");

    var trip = svc.CreateTrip(name, budget);
    svc.SelectTrip(trip.Id);

    MenuRenderer.ShowMessage($"Created + selected trip: {trip.Name}");
}

static void SelectTrip(TripService svc)
{
    var trips = svc.GetTrips();
    if (trips.Count == 0)
        throw new InvalidOperationException("No trips to select.");

    MenuRenderer.ShowTrips(trips);

    Console.Write("Select trip #: ");
    var s = (Console.ReadLine() ?? "").Trim();

    if (!int.TryParse(s, out var idx) || idx < 1 || idx > trips.Count)
        throw new ArgumentException("Invalid selection.");

    svc.SelectTrip(trips[idx - 1].Id);
    MenuRenderer.ShowMessage($"Selected trip: {trips[idx - 1].Name}");
}

static void AddStay(TripService svc)
{
    Console.Write("City: ");
    var city = (Console.ReadLine() ?? "").Trim();

    Console.Write("Country: ");
    var country = (Console.ReadLine() ?? "").Trim();

    Console.Write("Start date (YYYY-MM-DD) or blank: ");
    var startStr = (Console.ReadLine() ?? "").Trim();

    Console.Write("End date (YYYY-MM-DD) or blank: ");
    var endStr = (Console.ReadLine() ?? "").Trim();

    DateTime? start = string.IsNullOrWhiteSpace(startStr) ? null : ParseDate(startStr);
    DateTime? end = string.IsNullOrWhiteSpace(endStr) ? null : ParseDate(endStr);

    if ((start.HasValue && !end.HasValue) || (!start.HasValue && end.HasValue))
        throw new ArgumentException("Either provide both start and end date, or neither.");

    svc.AddStay(city, country, start, end);
    MenuRenderer.ShowMessage("Stay added.");
}

static StaySummary? SelectStay(TripService svc)
{
    var stays = svc.GetStays();
    if (stays.Count == 0)
        throw new InvalidOperationException("No stays found. Add a stay first.");

    MenuRenderer.ShowStays(stays);

    Console.Write("Select stay #: ");
    var input = (Console.ReadLine() ?? "").Trim();

    if (!int.TryParse(input, out var idx) || idx < 1 || idx > stays.Count)
        throw new ArgumentException("Invalid selection.");

    var selected = stays[idx - 1];
    MenuRenderer.ShowMessage($"Selected stay: {selected.DisplayKey}");
    return selected;
}

static StaySummary RequireActiveStay(StaySummary? activeStay)
{
    return activeStay ?? throw new InvalidOperationException("No active stay selected.");
}

static void SetStayPlace(TripService svc, ref StaySummary? activeStay)
{
    var stay = RequireActiveStay(activeStay);

    Console.Write("New city: ");
    var city = (Console.ReadLine() ?? "").Trim();

    Console.Write("New country: ");
    var country = (Console.ReadLine() ?? "").Trim();

    svc.UpdateStayPlace(stay.Id, city, country);
    activeStay = RefreshActiveStay(svc, stay.Id);

    MenuRenderer.ShowMessage("Stay place updated.");
}

static void SetStayStartDate(TripService svc, ref StaySummary? activeStay)
{
    var stay = RequireActiveStay(activeStay);

    Console.Write("New start date (YYYY-MM-DD): ");
    var input = (Console.ReadLine() ?? "").Trim();

    var startDate = ParseDate(input);

    svc.UpdateStayStartDate(stay.Id, startDate);
    activeStay = RefreshActiveStay(svc, stay.Id);

    MenuRenderer.ShowMessage("Stay start date updated.");
}

static void SetStayEndDate(TripService svc, ref StaySummary? activeStay)
{
    var stay = RequireActiveStay(activeStay);

    Console.Write("New end date (YYYY-MM-DD): ");
    var input = (Console.ReadLine() ?? "").Trim();

    var endDate = ParseDate(input);

    svc.UpdateStayEndDate(stay.Id, endDate);
    activeStay = RefreshActiveStay(svc, stay.Id);

    MenuRenderer.ShowMessage("Stay end date updated.");
}

static void DeleteActiveStay(TripService svc, ref StaySummary? activeStay)
{
    var stay = RequireActiveStay(activeStay);

    Console.Write($"Type DELETE to remove '{stay.DisplayKey}': ");
    var confirm = (Console.ReadLine() ?? "").Trim();

    if (!string.Equals(confirm, "DELETE", StringComparison.Ordinal))
    {
        MenuRenderer.ShowMessage("Delete cancelled.");
        return;
    }

    svc.DeleteStay(stay.Id);
    activeStay = null;

    MenuRenderer.ShowMessage("Stay deleted.");
}

static void AddExpenseToStay(TripService svc, StaySummary activeStay)
{
    Console.Write("Expense name: ");
    var name = (Console.ReadLine() ?? "").Trim();

    Console.Write("Amount (e.g. 25.50): ");
    var amtStr = (Console.ReadLine() ?? "").Trim();
    if (!decimal.TryParse(amtStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        throw new ArgumentException("Invalid amount.");

    var category = PromptExpenseCategory();

    Console.Write("Note (optional): ");
    var note = Console.ReadLine();

    svc.AddExpenseToStay(activeStay.Id, name, amount, category, note);
    MenuRenderer.ShowMessage("Expense added.");
}

static StaySummary? RefreshActiveStay(TripService svc, Guid stayId)
{
    return svc.GetStays().FirstOrDefault(s => s.Id == stayId);
}

static ExpenseCategory PromptExpenseCategory()
{
    var categories = Enum.GetValues<ExpenseCategory>();

    while (true)
    {
        Console.WriteLine("Expense categories:");
        for (int i = 0; i < categories.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {categories[i]}");
        }

        Console.Write("Choose category #: ");
        var input = (Console.ReadLine() ?? "").Trim();

        if (int.TryParse(input, out var index) &&
            index >= 1 &&
            index <= categories.Length)
        {
            return categories[index - 1];
        }

        if (Enum.TryParse<ExpenseCategory>(input, ignoreCase: true, out var category))
        {
            return category;
        }

        Console.WriteLine("Invalid category. Try again.");
        Console.WriteLine();
    }
}

static void ListExpenses(TripService svc, StaySummary activeStay)
{
    var expenses = svc.GetExpensesForStay(activeStay.Id);
    MenuRenderer.ShowExpenses(expenses);
}

static void AddExpense(TripService svc, StaySummary activeStay)
{
    Console.Write("Expense name: ");
    var name = (Console.ReadLine() ?? "").Trim();

    Console.Write("Amount (e.g. 25.50): ");
    var amtStr = (Console.ReadLine() ?? "").Trim();
    if (!decimal.TryParse(amtStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        throw new ArgumentException("Invalid amount.");

    var category = PromptExpenseCategory();

    Console.Write("Notes (optional): ");
    var notes = Console.ReadLine();

    svc.AddExpenseToStay(activeStay.Id, name, amount, category, notes);

    MenuRenderer.ShowMessage("Expense added.");
}

static ExpenseSummary RequireActiveExpense(ExpenseSummary? activeExpense)
{
    return activeExpense ?? throw new InvalidOperationException("No active expense selected.");
}

static void DeleteExpense(TripService svc, StaySummary activeStay)
{
    var expenses = svc.GetExpensesForStay(activeStay.Id);

    if (expenses.Count == 0)
        throw new InvalidOperationException("No expenses found.");

    MenuRenderer.ShowExpenses(expenses);

    Console.Write("Select expense #: ");
    var input = (Console.ReadLine() ?? "").Trim();

    if (!int.TryParse(input, out var idx) || idx < 1 || idx > expenses.Count)
        throw new ArgumentException("Invalid selection.");

    var selected = expenses[idx - 1];

    Console.Write($"Type DELETE to remove '{selected.Name}': ");
    var confirm = (Console.ReadLine() ?? "").Trim();

    if (!string.Equals(confirm, "DELETE", StringComparison.Ordinal))
    {
        MenuRenderer.ShowMessage("Delete cancelled.");
        return;
    }

    svc.DeleteExpense(activeStay.Id, selected.Id);
    MenuRenderer.ShowMessage("Expense deleted.");
}

static ExpenseSummary? SelectExpense(TripService svc, StaySummary activeStay)
{
    var expenses = svc.GetExpensesForStay(activeStay.Id);

    if (expenses.Count == 0)
        throw new InvalidOperationException("No expenses found.");

    MenuRenderer.ShowExpenses(expenses);

    Console.Write("Select expense #: ");
    var input = (Console.ReadLine() ?? "").Trim();

    if (!int.TryParse(input, out var idx) || idx < 1 || idx > expenses.Count)
        throw new ArgumentException("Invalid selection.");

    var selected = expenses[idx - 1];
    MenuRenderer.ShowMessage($"Selected expense: {selected.Name}");

    return selected;
}

static void RenameExpense(TripService svc, StaySummary activeStay, ref ExpenseSummary? activeExpense)
{
    var expense = RequireActiveExpense(activeExpense);

    Console.Write("New expense title: ");
    var newTitle = (Console.ReadLine() ?? "").Trim();

    svc.UpdateExpenseTitle(activeStay.Id, expense.Id, newTitle);
    activeExpense = RefreshActiveExpense(svc, activeStay.Id, expense.Id);

    MenuRenderer.ShowMessage("Expense title updated.");
}

static void UpdateExpenseAmount(TripService svc, StaySummary activeStay, ref ExpenseSummary? activeExpense)
{
    var expense = RequireActiveExpense(activeExpense);

    Console.Write("New amount (e.g. 25.50): ");
    var amtStr = (Console.ReadLine() ?? "").Trim();
    if (!decimal.TryParse(amtStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var newAmount))
        throw new ArgumentException("Invalid amount.");

    svc.UpdateExpenseAmount(activeStay.Id, expense.Id, newAmount);
    activeExpense = RefreshActiveExpense(svc, activeStay.Id, expense.Id);

    MenuRenderer.ShowMessage("Expense URL updated.");
}

static void UpdateExpenseNotes(TripService svc, StaySummary activeStay, ref ExpenseSummary? activeExpense)
{
    var expense = RequireActiveExpense(activeExpense);

    Console.Write("New notes (blank clears notes): ");
    var newNotes = Console.ReadLine();

    svc.UpdateExpenseNotes(activeStay.Id, expense.Id, newNotes);
    activeExpense = RefreshActiveExpense(svc, activeStay.Id, expense.Id);

    MenuRenderer.ShowMessage("Expense notes updated.");
}

static void DeleteActiveExpense(TripService svc, StaySummary activeStay, ref ExpenseSummary? activeExpense)
{
    if (activeExpense is null)
        throw new InvalidOperationException("No active expense selected.");

    Console.Write($"Type DELETE to remove '{activeExpense.Name}': ");
    var confirm = (Console.ReadLine() ?? "").Trim();

    if (!string.Equals(confirm, "DELETE", StringComparison.Ordinal))
    {
        MenuRenderer.ShowMessage("Delete cancelled.");
        return;
    }

    svc.DeleteExpense(activeStay.Id, activeExpense.Id);
    activeExpense = null;

    MenuRenderer.ShowMessage("Expense deleted.");
}

static ExpenseSummary? RefreshActiveExpense(TripService svc, Guid stayId, Guid expenseId)
{
    return svc.GetExpensesForStay(stayId).FirstOrDefault(b => b.Id == expenseId);
}

static void ListBookmarks(TripService svc, StaySummary activeStay)
{
    var bookmarks = svc.GetBookmarksForStay(activeStay.Id);
    MenuRenderer.ShowBookmarks(bookmarks);
}

static void AddBookmark(TripService svc, StaySummary activeStay)
{
    Console.Write("Bookmark title: ");
    var title = (Console.ReadLine() ?? "").Trim();

    Console.Write("Bookmark URL: ");
    var url = (Console.ReadLine() ?? "").Trim();

    Console.Write("Notes (optional): ");
    var notes = Console.ReadLine();

    svc.AddBookmarkToStay(activeStay.Id, title, url, notes);

    MenuRenderer.ShowMessage("Bookmark added.");
}

static BookmarkSummary RequireActiveBookmark(BookmarkSummary? activeBookmark)
{
    return activeBookmark ?? throw new InvalidOperationException("No active bookmark selected.");
}

static void DeleteBookmark(TripService svc, StaySummary activeStay)
{
    var bookmarks = svc.GetBookmarksForStay(activeStay.Id);

    if (bookmarks.Count == 0)
        throw new InvalidOperationException("No bookmarks found.");

    MenuRenderer.ShowBookmarks(bookmarks);

    Console.Write("Select bookmark #: ");
    var input = (Console.ReadLine() ?? "").Trim();

    if (!int.TryParse(input, out var idx) || idx < 1 || idx > bookmarks.Count)
        throw new ArgumentException("Invalid selection.");

    var selected = bookmarks[idx - 1];

    Console.Write($"Type DELETE to remove '{selected.Title}': ");
    var confirm = (Console.ReadLine() ?? "").Trim();

    if (!string.Equals(confirm, "DELETE", StringComparison.Ordinal))
    {
        MenuRenderer.ShowMessage("Delete cancelled.");
        return;
    }

    svc.DeleteBookmark(activeStay.Id, selected.Id);
    MenuRenderer.ShowMessage("Bookmark deleted.");
}

static BookmarkSummary? SelectBookmark(TripService svc, StaySummary activeStay)
{
    var bookmarks = svc.GetBookmarksForStay(activeStay.Id);

    if (bookmarks.Count == 0)
        throw new InvalidOperationException("No bookmarks found.");

    MenuRenderer.ShowBookmarks(bookmarks);

    Console.Write("Select bookmark #: ");
    var input = (Console.ReadLine() ?? "").Trim();

    if (!int.TryParse(input, out var idx) || idx < 1 || idx > bookmarks.Count)
        throw new ArgumentException("Invalid selection.");

    var selected = bookmarks[idx - 1];
    MenuRenderer.ShowMessage($"Selected bookmark: {selected.Title}");

    return selected;
}

static void RenameBookmark(TripService svc, StaySummary activeStay, ref BookmarkSummary? activeBookmark)
{
    var bookmark = RequireActiveBookmark(activeBookmark);

    Console.Write("New bookmark title: ");
    var newTitle = (Console.ReadLine() ?? "").Trim();

    svc.UpdateBookmarkTitle(activeStay.Id, bookmark.Id, newTitle);
    activeBookmark = RefreshActiveBookmark(svc, activeStay.Id, bookmark.Id);

    MenuRenderer.ShowMessage("Bookmark title updated.");
}

static void UpdateBookmarkUrl(TripService svc, StaySummary activeStay, ref BookmarkSummary? activeBookmark)
{
    var bookmark = RequireActiveBookmark(activeBookmark);

    Console.Write("New bookmark URL: ");
    var newUrl = (Console.ReadLine() ?? "").Trim();

    svc.UpdateBookmarkUrl(activeStay.Id, bookmark.Id, newUrl);
    activeBookmark = RefreshActiveBookmark(svc, activeStay.Id, bookmark.Id);

    MenuRenderer.ShowMessage("Bookmark URL updated.");
}

static void UpdateBookmarkNotes(TripService svc, StaySummary activeStay, ref BookmarkSummary? activeBookmark)
{
    var bookmark = RequireActiveBookmark(activeBookmark);

    Console.Write("New notes (blank clears notes): ");
    var newNotes = Console.ReadLine();

    svc.UpdateBookmarkNotes(activeStay.Id, bookmark.Id, newNotes);
    activeBookmark = RefreshActiveBookmark(svc, activeStay.Id, bookmark.Id);

    MenuRenderer.ShowMessage("Bookmark notes updated.");
}

static void DeleteActiveBookmark(TripService svc, StaySummary activeStay, ref BookmarkSummary? activeBookmark)
{
    if (activeBookmark is null)
        throw new InvalidOperationException("No active bookmark selected.");

    Console.Write($"Type DELETE to remove '{activeBookmark.Title}': ");
    var confirm = (Console.ReadLine() ?? "").Trim();

    if (!string.Equals(confirm, "DELETE", StringComparison.Ordinal))
    {
        MenuRenderer.ShowMessage("Delete cancelled.");
        return;
    }

    svc.DeleteBookmark(activeStay.Id, activeBookmark.Id);
    activeBookmark = null;

    MenuRenderer.ShowMessage("Bookmark deleted.");
}

static BookmarkSummary? RefreshActiveBookmark(TripService svc, Guid stayId, Guid bookmarkId)
{
    return svc.GetBookmarksForStay(stayId).FirstOrDefault(b => b.Id == bookmarkId);
}

static void SeedDemoData(TripService svc)
{
    var trip = svc.CreateTrip("Seed: Japan 2026", 5000m);
    svc.SelectTrip(trip.Id);

    svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));
    svc.AddStay("Osaka", "Japan", new DateTime(2026, 1, 14), new DateTime(2026, 1, 16));
    svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 16), new DateTime(2026, 1, 20));

    var stays = svc.GetStays();
    if (stays.Count > 0)
        svc.AddExpenseToStay(stays[0].Id, "Meals", 180m, ExpenseCategory.Food, "Sushi + ramen");

    MenuRenderer.ShowMessage("Seeded demo trip.");
}

static DateTime ParseDate(string s)
{
    if (!DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        throw new ArgumentException("Invalid date format. Use YYYY-MM-DD.");

    return dt.Date;
}