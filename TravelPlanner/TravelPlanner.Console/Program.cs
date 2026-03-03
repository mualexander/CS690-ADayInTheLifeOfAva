using System;
using System.Linq;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;

var repo = new InMemoryTripRepository();
var ctx = new InMemoryTripContext(repo);
var svc = new TripService(repo, ctx);

// Create + select trip
var trip = svc.CreateTrip("Japan 2026", 5000m);
svc.SelectTrip(trip.Id);

// Add locations
svc.AddLocation("Tokyo", "Japan");
svc.AddLocation("Osaka", "Japan");

// List locations (UI gets IDs via summaries)
var locations = svc.GetLocations();
for (int i = 0; i < locations.Count; i++)
{
    var l = locations[i];
    Console.WriteLine($"{i + 1}. {l.Name}, {l.Country} (spent {l.TotalSpent:0.00})");
}

// Choose Tokyo by name for demo purposes (normally you’d read user input)
var tokyoId = locations.First(l => l.Name == "Tokyo").Id;

// Add expense through service
svc.AddExpenseToLocation(tokyoId, DateTime.UtcNow.Date, 180m, ExpenseCategory.Food, "Sushi + ramen");

Console.WriteLine($"Trip spent: {svc.GetTripTotalSpent():0.00}");
Console.WriteLine($"Budget left: {svc.GetTripRemainingBudget():0.00}");