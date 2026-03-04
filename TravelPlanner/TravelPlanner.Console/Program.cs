using System;
using System.Linq;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;

var repo = new InMemoryTripRepository();
var ctx = new InMemoryTripContext(repo);
var svc = new TripService(repo, ctx);

// Seed a couple trips (for now)
var t1 = svc.CreateTrip("Japan 2026", 5000m);
svc.SelectTrip(t1.Id);

// stays (Tokyo -> Osaka -> Tokyo)
svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));
svc.AddStay("Osaka", "Japan", new DateTime(2026, 1, 14), new DateTime(2026, 1, 16));
svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 16), new DateTime(2026, 1, 20));

var stays1 = svc.GetStays();
svc.AddExpenseToStay(stays1[0].Id, DateTime.UtcNow.Date, 180m, ExpenseCategory.Food, "Sushi + ramen");

var t2 = svc.CreateTrip("Austin Weekend", 1200m);
svc.SelectTrip(t2.Id);
svc.AddStay("Austin", "USA");

// ---- UI: select trip ----
Console.WriteLine("Trips:");
var trips = svc.GetTrips();
for (int i = 0; i < trips.Count; i++)
{
    var tr = trips[i];
    Console.WriteLine(
        $"{i + 1}. {tr.Name} | Budget {tr.TotalBudget:0.00} | Spent {tr.TotalSpent:0.00} | Left {tr.RemainingBudget:0.00} | Stays {tr.StayCount}"
    );
}

Console.Write("Select trip #: ");
if (!int.TryParse(Console.ReadLine(), out var tripChoice) || tripChoice < 1 || tripChoice > trips.Count)
{
    Console.WriteLine("Invalid selection.");
    return;
}

var selectedTripId = trips[tripChoice - 1].Id;
svc.SelectTrip(selectedTripId);

// ---- UI: list stays for active trip ----
Console.WriteLine();
Console.WriteLine("Stays:");
var stays = svc.GetStays();
if (stays.Count == 0)
{
    Console.WriteLine("(none)");
    return;
}

for (int i = 0; i < stays.Count; i++)
{
    var s = stays[i];
    var datePart =
        (s.StartDate.HasValue && s.EndDate.HasValue)
            ? $" ({s.StartDate:yyyy-MM-dd} → {s.EndDate:yyyy-MM-dd})"
            : "";

    Console.WriteLine($"{i + 1}. {s.City}, {s.Country}{datePart} | Spent {s.TotalSpent:0.00}");
}

Console.Write("Select stay # to add a sample expense: ");
if (!int.TryParse(Console.ReadLine(), out var stayChoice) || stayChoice < 1 || stayChoice > stays.Count)
{
    Console.WriteLine("Invalid selection.");
    return;
}

var selectedStayId = stays[stayChoice - 1].Id;

// ---- UI: add expense (hard-coded sample for now) ----
svc.AddExpenseToStay(selectedStayId, DateTime.UtcNow.Date, 25m, ExpenseCategory.Food, "Coffee + snack");

Console.WriteLine();
Console.WriteLine("Updated trip totals:");
Console.WriteLine($"Spent: {svc.GetTripTotalSpent():0.00}");
Console.WriteLine($"Left:  {svc.GetTripRemainingBudget():0.00}");