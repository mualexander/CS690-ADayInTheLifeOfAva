using System;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;

var repo = new InMemoryTripRepository();
var ctx = new InMemoryTripContext(repo);
var svc = new TripService(repo, ctx);

// Seed a couple trips (for now)
var t1 = svc.CreateTrip("Japan 2026", 5000m);
svc.SelectTrip(t1.Id);
svc.AddLocation("Tokyo", "Japan");
svc.AddLocation("Osaka", "Japan");

var locs = svc.GetLocations();
svc.AddExpenseToLocation(locs[0].Id, DateTime.UtcNow.Date, 180m, ExpenseCategory.Food, "Sushi + ramen");

var t2 = svc.CreateTrip("Austin Weekend", 1200m);
svc.SelectTrip(t2.Id);
svc.AddLocation("Austin", "USA");

// ---- UI: select trip ----
Console.WriteLine("Trips:");
var trips = svc.GetTrips();
for (int i = 0; i < trips.Count; i++)
{
    var tr = trips[i];
    Console.WriteLine(
        $"{i + 1}. {tr.Name} | Budget {tr.TotalBudget:0.00} | Spent {tr.TotalSpent:0.00} | Left {tr.RemainingBudget:0.00} | Locations {tr.LocationCount}"
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

// ---- UI: list locations for active trip ----
Console.WriteLine();
Console.WriteLine("Locations:");
var locations = svc.GetLocations();
if (locations.Count == 0)
{
    Console.WriteLine("(none)");
    return;
}

for (int i = 0; i < locations.Count; i++)
{
    var l = locations[i];
    Console.WriteLine($"{i + 1}. {l.Name}, {l.Country} | Spent {l.TotalSpent:0.00}");
}

Console.Write("Select location # to add a sample expense: ");
if (!int.TryParse(Console.ReadLine(), out var locChoice) || locChoice < 1 || locChoice > locations.Count)
{
    Console.WriteLine("Invalid selection.");
    return;
}

var selectedLocationId = locations[locChoice - 1].Id;

// ---- UI: add expense (hard-coded sample for now) ----
svc.AddExpenseToLocation(selectedLocationId, DateTime.UtcNow.Date, 25m, ExpenseCategory.Food, "Coffee + snack");

Console.WriteLine();
Console.WriteLine("Updated trip totals:");
Console.WriteLine($"Spent: {svc.GetTripTotalSpent():0.00}");
Console.WriteLine($"Left:  {svc.GetTripRemainingBudget():0.00}");