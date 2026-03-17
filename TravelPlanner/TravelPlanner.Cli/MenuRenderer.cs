using System;
using System.Collections.Generic;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class MenuRenderer
{
    public static void ShowHeader(Trip? activeTrip, StaySummary? activeStay = null)
    {
        WriteTitle("TravelPlanner");

        if (activeTrip is null)
        {
            WriteLine("Active trip: (none)");
        }
        else
        {
            WriteLine($"Active trip: {activeTrip.Name}");
            WriteLine($"Budget: {activeTrip.TotalBudget:0.00}");
            WriteLine($"Spent:  {activeTrip.TotalSpent():0.00}");
            WriteLine($"Left:   {activeTrip.RemainingBudget():0.00}");
        }

        if (activeStay is not null)
        {
            WriteLine($"Active stay: {activeStay.DisplayKey}");
        }

        BlankLine();
    }

    public static void ShowMainMenu()
    {
        ShowMenu(
            "Main Menu",
            ("L", "List trips"),
            ("C", "Create trip"),
            ("S", "Select trip"),
            ("T", "Seed test data"),
            ("Q", "Quit")
        );
    }

    public static void ShowTripMenu()
    {
        ShowMenu(
            "Trip Menu",
            ("V", "View trip summary"),
            ("L", "List stays"),
            ("A", "Add stay"),
            ("S", "Select stay"),
            ("R", "Rename trip"),
            ("B", "Update budget"),
            ("X", "Archive trip"),
            ("Q", "Back")
        );
    }

    public static void ShowStayMenu()
    {
        ShowMenu(
            "Stay Menu",
            ("V", "View stay details"),
            ("E", "Expenses"),
            ("B", "Bookmarks"),
            ("F", "Flights"),
            ("L", "Lodging"),
            ("P", "Set place"),
            ("I", "Set start date"),
            ("O", "Set end date"),
            ("X", "Delete stay"),
            ("Q", "Back")
        );
    }

    public static void ShowExpenseMenu()
    {
        ShowMenu(
            "Expense Menu",
            ("L", "List expenses"),
            ("A", "Add expense"),
            ("S", "Select expense"),
            ("X", "Delete expense"),
            ("Q", "Back")
        );
    }

    public static void ShowExpenseDetailMenu()
    {
        ShowMenu(
            "Expense Detail Menu",
            ("V", "View expense details"),
            ("R", "Rename expense"),
            ("U", "Update amount"),
            ("N", "Update notes"),
            ("X", "Delete expense"),
            ("Q", "Back")
        );
    }
    public static void ShowBookmarkMenu()
    {
        ShowMenu(
            "Bookmark Menu",
            ("L", "List bookmarks"),
            ("A", "Add bookmark"),
            ("S", "Select bookmark"),
            ("X", "Delete bookmark"),
            ("Q", "Back")
        );
    }

    public static void ShowBookmarkDetailMenu()
    {
        ShowMenu(
            "Bookmark Detail Menu",
            ("V", "View bookmark details"),
            ("R", "Rename bookmark"),
            ("U", "Update URL"),
            ("N", "Update notes"),
            ("X", "Delete bookmark"),
            ("Q", "Back")
        );
    }

    public static void ShowFlightOptionMenu()
    {
        ShowMenu(
            "Flight Option Menu",
            ("L", "List flight options"),
            ("A", "Add flight option"),
            ("S", "Select flight option"),
            ("D", "Delete flight option"),
            ("Q", "Back")
        );
    }

    public static void ShowFlightOptionDetailMenu()
    {
        ShowMenu(
            "Flight Option Detail Menu",
            ("V", "View flight option details"),
            ("Q", "Back")
        );
    }

    public static void ShowLodgingOptionMenu()
    {
        ShowMenu(
            "Lodging Option Menu",
            ("L", "List lodging options"),
            ("A", "Add lodging option"),
            ("S", "Select lodging option"),
            ("D", "Delete lodging option"),
            ("Q", "Back")
        );
    }

    public static void ShowLodgingOptionDetailMenu()
    {
        ShowMenu(
            "Lodging Option Detail Menu",
            ("V", "View lodging option details"),
            ("Q", "Back")
        );
    }


    public static void ShowTrips(IReadOnlyList<TripSummary> trips)
    {
        WriteTitle("Trips");

        if (trips.Count == 0)
        {
            WriteLine("(no trips yet)");
            return;
        }

        for (int i = 0; i < trips.Count; i++)
        {
            var t = trips[i];
            WriteLine(
                $"{i + 1}. {t.Name} | Budget {t.TotalBudget:0.00} | Spent {t.TotalSpent:0.00} | Left {t.RemainingBudget:0.00} | Stays {t.StayCount}"
            );
        }
    }

    public static void ShowStays(IReadOnlyList<StaySummary> stays)
    {
        WriteTitle("Stays");

        if (stays.Count == 0)
        {
            WriteLine("(no stays yet)");
            return;
        }

        for (int i = 0; i < stays.Count; i++)
        {
            var s = stays[i];
            WriteLine($"{i + 1}. {s.DisplayKey} | Spent {s.TotalSpent:0.00}");
        }
    }

    public static void ShowTripSummary(Trip trip)
    {
        WriteTitle("Trip Summary");
        WriteLine($"Name:   {trip.Name}");
        WriteLine($"Budget: {trip.TotalBudget:0.00}");
        WriteLine($"Spent:  {trip.TotalSpent():0.00}");
        WriteLine($"Left:   {trip.RemainingBudget():0.00}");
        WriteLine($"Stays:  {trip.Stays.Count}");
    }

    public static void ShowStayDetails(StaySummary stay)
    {
        WriteTitle("Stay Details");
        WriteLine($"Stay:    {stay.DisplayKey}");
        WriteLine($"City:    {stay.City}");
        WriteLine($"Country: {stay.Country}");
        WriteLine($"Spent:   {stay.TotalSpent:0.00}");

        if (stay.StartDate.HasValue && stay.EndDate.HasValue)
        {
            WriteLine($"Dates:   {stay.StartDate:yyyy-MM-dd} to {stay.EndDate:yyyy-MM-dd}");
        }
    }

    public static void ShowExpenses(IReadOnlyList<ExpenseSummary> expenses)
    {
        WriteTitle("Expenses");

        if (expenses.Count == 0)
        {
            WriteLine("(no expenses yet)");
            return;
        }

        for (int i = 0; i < expenses.Count; i++)
        {
            var e = expenses[i];
            WriteLine($"{i + 1}. {e.Name} | {e.Amount}");
            if (!string.IsNullOrWhiteSpace(e.Notes))
            {
                WriteLine($"   Notes: {e.Notes}");
            }
        }
    }

    public static void ShowExpenseDetails(ExpenseSummary expense)
    {
        WriteTitle("Expense Details");
        WriteLine($"Name:     {expense.Name}");
        WriteLine($"Amount:       {expense.Amount}");
        WriteLine($"CreatedAt: {expense.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        WriteLine($"Notes:     {expense.Notes ?? "(none)"}");
    }

    public static void ShowBookmarks(IReadOnlyList<BookmarkSummary> bookmarks)
    {
        WriteTitle("Bookmarks");

        if (bookmarks.Count == 0)
        {
            WriteLine("(no bookmarks yet)");
            return;
        }

        for (int i = 0; i < bookmarks.Count; i++)
        {
            var b = bookmarks[i];
            WriteLine($"{i + 1}. {b.Title} | {b.Url}");
            if (!string.IsNullOrWhiteSpace(b.Notes))
            {
                WriteLine($"   Notes: {b.Notes}");
            }
        }
    }

    public static void ShowBookmarkDetails(BookmarkSummary bookmark)
    {
        WriteTitle("Bookmark Details");
        WriteLine($"Title:     {bookmark.Title}");
        WriteLine($"URL:       {bookmark.Url}");
        WriteLine($"CreatedAt: {bookmark.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        WriteLine($"Notes:     {bookmark.Notes ?? "(none)"}");
    }

    public static void ShowFlightOptions(IReadOnlyList<FlightOptionSummary> options)
    {
        WriteTitle("Flight Options");

        if (options.Count == 0)
        {
            WriteLine("(no flight options yet)");
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            var f = options[i];
            WriteLine(
                $"{i + 1}. {f.FromAirportCode} -> {f.ToAirportCode} | " +
                $"{f.DepartTime:yyyy-MM-dd HH:mm} -> {f.ArriveTime:yyyy-MM-dd HH:mm}"
            );
        }
    }

    public static void ShowFlightOptionDetails(FlightOptionSummary option)
    {
        WriteTitle("Flight Option Details");
        WriteLine($"Route:        {option.FromAirportCode} -> {option.ToAirportCode}");
        WriteLine($"Depart:       {option.DepartTime:yyyy-MM-dd HH:mm}");
        WriteLine($"Arrive:       {option.ArriveTime:yyyy-MM-dd HH:mm}");
        WriteLine($"URL:          {option.Url}");
        WriteLine($"CreatedAt:    {option.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        WriteLine($"LastChecked:  {(option.LastCheckedAt.HasValue ? option.LastCheckedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" : "(never)")}");
        WriteLine($"Selected:     {option.IsSelected}");
    }

    public static void ShowLodgingOptions(IReadOnlyList<LodgingOptionSummary> options)
    {
        WriteTitle("Lodging Options");

        if (options.Count == 0)
        {
            WriteLine("(no lodging options yet)");
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            var l = options[i];
            WriteLine(
                $"{i + 1}. {l.PropertyName} | " +
                $"{l.CheckInDate:yyyy-MM-dd} -> {l.CheckOutDate:yyyy-MM-dd}"
            );
        }
    }

    public static void ShowLodgingOptionDetails(LodgingOptionSummary option)
    {
        WriteTitle("Lodging Option Details");
        WriteLine($"Property:     {option.PropertyName}");
        WriteLine($"Check-in:     {option.CheckInDate:yyyy-MM-dd}");
        WriteLine($"Check-out:    {option.CheckOutDate:yyyy-MM-dd}");
        WriteLine($"URL:          {option.Url}");
        WriteLine($"CreatedAt:    {option.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        WriteLine($"LastChecked:  {(option.LastCheckedAt.HasValue ? option.LastCheckedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" : "(never)")}");
        WriteLine($"Selected:     {option.IsSelected}");
    }

    public static void ShowError(string message)
    {
        WriteLine($"ERROR: {message}");
    }

    public static void ShowMessage(string message)
    {
        WriteLine(message);
    }

    public static void BlankLine()
    {
        System.Console.WriteLine();
    }

    private static void ShowMenu(string title, params (string Command, string Description)[] items)
    {
        WriteTitle(title);

        foreach (var item in items)
        {
            WriteLine($"{item.Command}) {item.Description}");
        }

        Write("Choice: ");
    }

    private static void WriteTitle(string title)
    {
        WriteLine(title);
        WriteLine(new string('-', 50));
    }

    private static void WriteLine(string value)
    {
        System.Console.WriteLine(value);
    }

    private static void Write(string value)
    {
        System.Console.Write(value);
    }
}