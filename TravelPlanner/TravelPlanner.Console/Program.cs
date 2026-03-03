using TravelPlanner.Core.Models;
using TravelPlanner.Core.Repositories;

var repo = new InMemoryTripRepository();

var trip = new Trip("Japan 2026", 5000m);
trip.AddLocation(new Location("Tokyo", "Japan"));
trip.AddLocation(new Location("Osaka", "Japan"));

var tokyo = trip.Locations.First(l => l.Name == "Tokyo");
tokyo.AddExpense(DateTime.UtcNow.Date, 180m, ExpenseCategory.Food, "Sushi + ramen");

repo.Add(trip);

// Fetch and report
var loaded = repo.GetById(trip.Id)!;
Console.WriteLine($"Trip: {loaded.Name}");
Console.WriteLine($"Budget: {loaded.TotalBudget:0.00}");
Console.WriteLine($"Spent:  {loaded.TotalSpent():0.00}");
Console.WriteLine($"Left:   {loaded.RemainingBudget():0.00}");