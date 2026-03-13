using System.Globalization;
using System.Linq;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class ConsolePrompts
{
    public static void CreateTrip(TripService svc)
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

    public static void SelectTrip(TripService svc)
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

    public static void AddStay(TripService svc)
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

    public static StaySummary SelectStay(TripService svc)
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

    public static StaySummary RequireActiveStay(StaySummary? activeStay) =>
        activeStay ?? throw new InvalidOperationException("No active stay selected.");

    public static void SetStayPlace(TripService svc, ref StaySummary? activeStay)
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

    public static void SetStayStartDate(TripService svc, ref StaySummary? activeStay)
    {
        var stay = RequireActiveStay(activeStay);

        Console.Write("New start date (YYYY-MM-DD): ");
        var input = (Console.ReadLine() ?? "").Trim();

        var startDate = ParseDate(input);

        svc.UpdateStayStartDate(stay.Id, startDate);
        activeStay = RefreshActiveStay(svc, stay.Id);

        MenuRenderer.ShowMessage("Stay start date updated.");
    }

    public static void SetStayEndDate(TripService svc, ref StaySummary? activeStay)
    {
        var stay = RequireActiveStay(activeStay);

        Console.Write("New end date (YYYY-MM-DD): ");
        var input = (Console.ReadLine() ?? "").Trim();

        var endDate = ParseDate(input);

        svc.UpdateStayEndDate(stay.Id, endDate);
        activeStay = RefreshActiveStay(svc, stay.Id);

        MenuRenderer.ShowMessage("Stay end date updated.");
    }

    public static void DeleteActiveStay(TripService svc, ref StaySummary? activeStay)
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

    public static void AddExpense(TripService svc, StaySummary activeStay)
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

    public static void DeleteExpense(TripService svc, StaySummary activeStay)
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

    public static ExpenseSummary SelectExpense(TripService svc, StaySummary activeStay)
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

    public static ExpenseSummary RequireActiveExpense(ExpenseSummary? activeExpense) =>
        activeExpense ?? throw new InvalidOperationException("No active expense selected.");

    public static void RenameExpense(TripService svc, StaySummary activeStay, ref ExpenseSummary? activeExpense)
    {
        var expense = RequireActiveExpense(activeExpense);

        Console.Write("New expense title: ");
        var newTitle = (Console.ReadLine() ?? "").Trim();

        svc.UpdateExpenseTitle(activeStay.Id, expense.Id, newTitle);
        activeExpense = RefreshActiveExpense(svc, activeStay.Id, expense.Id);

        MenuRenderer.ShowMessage("Expense title updated.");
    }

    public static void UpdateExpenseAmount(TripService svc, StaySummary activeStay, ref ExpenseSummary? activeExpense)
    {
        var expense = RequireActiveExpense(activeExpense);

        Console.Write("New amount (e.g. 25.50): ");
        var amtStr = (Console.ReadLine() ?? "").Trim();
        if (!decimal.TryParse(amtStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var newAmount))
            throw new ArgumentException("Invalid amount.");

        svc.UpdateExpenseAmount(activeStay.Id, expense.Id, newAmount);
        activeExpense = RefreshActiveExpense(svc, activeStay.Id, expense.Id);

        MenuRenderer.ShowMessage("Expense amount updated.");
    }

    public static void UpdateExpenseNotes(TripService svc, StaySummary activeStay, ref ExpenseSummary? activeExpense)
    {
        var expense = RequireActiveExpense(activeExpense);

        Console.Write("New notes (blank clears notes): ");
        var newNotes = Console.ReadLine();

        svc.UpdateExpenseNotes(activeStay.Id, expense.Id, newNotes);
        activeExpense = RefreshActiveExpense(svc, activeStay.Id, expense.Id);

        MenuRenderer.ShowMessage("Expense notes updated.");
    }

    public static void DeleteActiveExpense(TripService svc, StaySummary activeStay, ref ExpenseSummary? activeExpense)
    {
        var expense = RequireActiveExpense(activeExpense);

        Console.Write($"Type DELETE to remove '{expense.Name}': ");
        var confirm = (Console.ReadLine() ?? "").Trim();

        if (!string.Equals(confirm, "DELETE", StringComparison.Ordinal))
        {
            MenuRenderer.ShowMessage("Delete cancelled.");
            return;
        }

        svc.DeleteExpense(activeStay.Id, expense.Id);
        activeExpense = null;

        MenuRenderer.ShowMessage("Expense deleted.");
    }

    public static void AddBookmark(TripService svc, StaySummary activeStay)
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

    public static void DeleteBookmark(TripService svc, StaySummary activeStay)
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

    public static BookmarkSummary SelectBookmark(TripService svc, StaySummary activeStay)
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

    public static BookmarkSummary RequireActiveBookmark(BookmarkSummary? activeBookmark) =>
        activeBookmark ?? throw new InvalidOperationException("No active bookmark selected.");

    public static void RenameBookmark(TripService svc, StaySummary activeStay, ref BookmarkSummary? activeBookmark)
    {
        var bookmark = RequireActiveBookmark(activeBookmark);

        Console.Write("New bookmark title: ");
        var newTitle = (Console.ReadLine() ?? "").Trim();

        svc.UpdateBookmarkTitle(activeStay.Id, bookmark.Id, newTitle);
        activeBookmark = RefreshActiveBookmark(svc, activeStay.Id, bookmark.Id);

        MenuRenderer.ShowMessage("Bookmark title updated.");
    }

    public static void UpdateBookmarkUrl(TripService svc, StaySummary activeStay, ref BookmarkSummary? activeBookmark)
    {
        var bookmark = RequireActiveBookmark(activeBookmark);

        Console.Write("New bookmark URL: ");
        var newUrl = (Console.ReadLine() ?? "").Trim();

        svc.UpdateBookmarkUrl(activeStay.Id, bookmark.Id, newUrl);
        activeBookmark = RefreshActiveBookmark(svc, activeStay.Id, bookmark.Id);

        MenuRenderer.ShowMessage("Bookmark URL updated.");
    }

    public static void UpdateBookmarkNotes(TripService svc, StaySummary activeStay, ref BookmarkSummary? activeBookmark)
    {
        var bookmark = RequireActiveBookmark(activeBookmark);

        Console.Write("New notes (blank clears notes): ");
        var newNotes = Console.ReadLine();

        svc.UpdateBookmarkNotes(activeStay.Id, bookmark.Id, newNotes);
        activeBookmark = RefreshActiveBookmark(svc, activeStay.Id, bookmark.Id);

        MenuRenderer.ShowMessage("Bookmark notes updated.");
    }

    public static void DeleteActiveBookmark(TripService svc, StaySummary activeStay, ref BookmarkSummary? activeBookmark)
    {
        var bookmark = RequireActiveBookmark(activeBookmark);

        Console.Write($"Type DELETE to remove '{bookmark.Title}': ");
        var confirm = (Console.ReadLine() ?? "").Trim();

        if (!string.Equals(confirm, "DELETE", StringComparison.Ordinal))
        {
            MenuRenderer.ShowMessage("Delete cancelled.");
            return;
        }

        svc.DeleteBookmark(activeStay.Id, bookmark.Id);
        activeBookmark = null;

        MenuRenderer.ShowMessage("Bookmark deleted.");
    }

    public static void SeedDemoData(TripService svc)
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

    public static StaySummary? RefreshActiveStay(TripService svc, Guid stayId) =>
        svc.GetStays().FirstOrDefault(s => s.Id == stayId);

    public static ExpenseSummary? RefreshActiveExpense(TripService svc, Guid stayId, Guid expenseId) =>
        svc.GetExpensesForStay(stayId).FirstOrDefault(e => e.Id == expenseId);

    public static BookmarkSummary? RefreshActiveBookmark(TripService svc, Guid stayId, Guid bookmarkId) =>
        svc.GetBookmarksForStay(stayId).FirstOrDefault(b => b.Id == bookmarkId);

    public static DateTime ParseDate(string s)
    {
        if (!DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            throw new ArgumentException("Invalid date format. Use YYYY-MM-DD.");

        return dt.Date;
    }

    public static ExpenseCategory PromptExpenseCategory()
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
}