using System;
using System.Globalization;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;

const string DataPath = "data/trips.json";

var repo = new FileTripRepository(DataPath);
var ctx = new InMemoryTripContext(repo);
var svc = new TripService(repo, ctx);

Console.WriteLine($"TravelPlanner (data: {DataPath})");
Console.WriteLine();

while (true)
{
    var active = ctx.ActiveTrip;
    Console.WriteLine(active is null
        ? "Active trip: (none)"
        : $"Active trip: {active.Name} | Spent {svc.GetTripTotalSpent():0.00} | Left {svc.GetTripRemainingBudget():0.00}");

    Console.WriteLine();
    Console.WriteLine("Menu:");
    Console.WriteLine("L) List trips");
    Console.WriteLine("A) Add trip");
    Console.WriteLine("S) Select trip");
    Console.WriteLine("1) List stays (active trip)");
    Console.WriteLine("2) Add stay (active trip)");
    Console.WriteLine("3) Add expense to stay (active trip)");
    Console.WriteLine("0) Seed demo data (optional)");
    Console.WriteLine("Q) Quit");
    Console.Write("Choice: ");

    var choice = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
    Console.WriteLine();

    try
    {
        switch (choice)
        {
            case "L":
                ListTrips(svc);
                break;

            case "A":
                CreateTrip(svc);
                break;

            case "S":
                SelectTrip(svc);
                break;

            case "1":
                ListStays(svc);
                break;

            case "2":
                AddStay(svc);
                break;

            case "3":
                AddExpense(svc);
                break;

            case "0":
                SeedDemoData(svc);
                break;

            case "Q":
                Console.WriteLine("Bye.");
                return;

            default:
                Console.WriteLine("Unknown choice.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex.Message}");
    }

    Console.WriteLine();
}

static void ListTrips(TripService svc)
{
    var trips = svc.GetTrips();
    if (trips.Count == 0)
    {
        Console.WriteLine("(no trips yet)");
        return;
    }

    for (int i = 0; i < trips.Count; i++)
    {
        var t = trips[i];
        Console.WriteLine($"{i + 1}. {t.Name} | Budget {t.TotalBudget:0.00} | Spent {t.TotalSpent:0.00} | Left {t.RemainingBudget:0.00} | Stays {t.StayCount}");
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

    Console.WriteLine($"Created + selected trip: {trip.Name}");
}

static void SelectTrip(TripService svc)
{
    var trips = svc.GetTrips();
    if (trips.Count == 0)
        throw new InvalidOperationException("No trips to select.");

    for (int i = 0; i < trips.Count; i++)
        Console.WriteLine($"{i + 1}. {trips[i].Name}");

    Console.Write("Select trip #: ");
    var s = (Console.ReadLine() ?? "").Trim();

    if (!int.TryParse(s, out var idx) || idx < 1 || idx > trips.Count)
        throw new ArgumentException("Invalid selection.");

    svc.SelectTrip(trips[idx - 1].Id);
    Console.WriteLine($"Selected trip: {trips[idx - 1].Name}");
}

static void ListStays(TripService svc)
{
    var stays = svc.GetStays();
    if (stays.Count == 0)
    {
        Console.WriteLine("(no stays yet)");
        return;
    }

    for (int i = 0; i < stays.Count; i++)
    {
        var s = stays[i];
        Console.WriteLine($"{i + 1}. {s.DisplayKey} | Spent {s.TotalSpent:0.00}");
    }
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
    Console.WriteLine("Stay added.");
}

static void AddExpense(TripService svc)
{
    var stays = svc.GetStays();
    if (stays.Count == 0)
        throw new InvalidOperationException("No stays found. Add a stay first.");

    for (int i = 0; i < stays.Count; i++)
        Console.WriteLine($"{i + 1}. {stays[i].DisplayKey}");

    Console.Write("Select stay #: ");
    var selStr = (Console.ReadLine() ?? "").Trim();
    if (!int.TryParse(selStr, out var idx) || idx < 1 || idx > stays.Count)
        throw new ArgumentException("Invalid selection.");

    var stayId = stays[idx - 1].Id;

    Console.Write("Expense date (YYYY-MM-DD) [blank=today]: ");
    var dateStr = (Console.ReadLine() ?? "").Trim();
    var date = string.IsNullOrWhiteSpace(dateStr) ? DateTime.UtcNow.Date : ParseDate(dateStr);

    Console.Write("Amount (e.g. 25.50): ");
    var amtStr = (Console.ReadLine() ?? "").Trim();
    if (!decimal.TryParse(amtStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        throw new ArgumentException("Invalid amount.");

    Console.WriteLine("Category:");
    foreach (var cat in Enum.GetValues<ExpenseCategory>())
        Console.WriteLine($"- {cat}");

    Console.Write("Enter category: ");
    var catStr = (Console.ReadLine() ?? "").Trim();

    if (!Enum.TryParse<ExpenseCategory>(catStr, ignoreCase: true, out var category))
        category = ExpenseCategory.Other;

    Console.Write("Note (optional): ");
    var note = Console.ReadLine();

    svc.AddExpenseToStay(stayId, date, amount, category, note);
    Console.WriteLine("Expense added.");
}

static void SeedDemoData(TripService svc)
{
    // Explicit. Only runs if user chooses it.
    var trip = svc.CreateTrip("Seed: Japan 2026", 5000m);
    svc.SelectTrip(trip.Id);

    svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));
    svc.AddStay("Osaka", "Japan", new DateTime(2026, 1, 14), new DateTime(2026, 1, 16));
    svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 16), new DateTime(2026, 1, 20));

    var stays = svc.GetStays();
    if (stays.Count > 0)
        svc.AddExpenseToStay(stays[0].Id, DateTime.UtcNow.Date, 180m, ExpenseCategory.Food, "Sushi + ramen");

    Console.WriteLine("Seeded demo trip.");
}

static DateTime ParseDate(string s)
{
    if (!DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        throw new ArgumentException("Invalid date format. Use YYYY-MM-DD.");

    return dt.Date;
}