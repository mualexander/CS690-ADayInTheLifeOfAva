using System;
using System.Linq;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;
using Xunit;

namespace TravelPlanner.Core.Tests.Services;

public class TripServiceTests
{
    [Fact]
    public void CreateTrip_AddsTripToRepository()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);

        var fromRepo = repo.GetById(trip.Id);
        Assert.NotNull(fromRepo);
        Assert.Equal("Japan 2026", fromRepo!.Name);
        Assert.Equal(5000m, fromRepo.TotalBudget);
    }

    [Fact]
    public void SelectTrip_SetsActiveTripInContext()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);

        svc.SelectTrip(trip.Id);

        Assert.NotNull(ctx.ActiveTrip);
        Assert.Equal(trip.Id, ctx.ActiveTrip!.Id);
    }

    [Fact]
    public void SelectTrip_ThrowsIfTripNotFound()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() => svc.SelectTrip(Guid.NewGuid()));
    }

    [Fact]
    public void AddStay_ThrowsIfNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() => svc.AddStay("Tokyo", "Japan"));
    }

    [Fact]
    public void AddStay_AddsStayToActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));
        svc.AddStay("Osaka", "Japan", new DateTime(2026, 1, 14), new DateTime(2026, 1, 16));
        svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 16), new DateTime(2026, 1, 20)); // allow Tokyo twice

        var updated = repo.GetById(trip.Id)!;
        Assert.Equal(3, updated.Stays.Count);
        Assert.Equal(2, updated.Stays.Count(s => s.Place.City == "Tokyo" && s.Place.Country == "Japan"));
    }

    [Fact]
    public void GetStays_ReturnsSummariesWithIds()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        svc.AddStay("Tokyo", "Japan");
        var stays = svc.GetStays();

        Assert.Single(stays);
        Assert.Equal("Tokyo", stays[0].City);
        Assert.NotEqual(Guid.Empty, stays[0].Id); // required for selection
    }

    [Fact]
    public void AddExpenseToStay_ThrowsIfNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() =>
            svc.AddExpenseToStay(Guid.NewGuid(), DateTime.UtcNow.Date, 10m, ExpenseCategory.Food));
    }

    [Fact]
    public void AddExpenseToStay_ThrowsIfStayNotFound()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        Assert.Throws<InvalidOperationException>(() =>
            svc.AddExpenseToStay(Guid.NewGuid(), DateTime.UtcNow.Date, 10m, ExpenseCategory.Food));
    }

    [Fact]
    public void AddExpenseToStay_AddsExpenseAndUpdatesTotals()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddExpenseToStay(stayId, DateTime.UtcNow.Date, 180m, ExpenseCategory.Food, "Sushi");

        Assert.Equal(180m, svc.GetTripTotalSpent());
        Assert.Equal(4820m, svc.GetTripRemainingBudget());

        var updated = repo.GetById(trip.Id)!;
        var tokyoStay = updated.Stays.Single();
        Assert.Single(tokyoStay.Expenses);
        Assert.Equal(180m, tokyoStay.TotalSpent());
    }

    [Fact]
    public void GetTrips_ReturnsRollups()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 1000m);
        svc.SelectTrip(trip.Id);

        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddExpenseToStay(stayId, DateTime.UtcNow.Date, 200m, ExpenseCategory.Food);

        var trips = svc.GetTrips();
        var summary = trips.Single(t => t.Id == trip.Id);

        Assert.Equal("Japan 2026", summary.Name);
        Assert.Equal(1000m, summary.TotalBudget);
        Assert.Equal(200m, summary.TotalSpent);
        Assert.Equal(800m, summary.RemainingBudget);
        Assert.Equal(1, summary.StayCount);
    }

    [Fact]
    public void SwitchingActiveTrip_ChangesWhichTripIsMutated()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var a = svc.CreateTrip("A", 1000m);
        var b = svc.CreateTrip("B", 1000m);

        svc.SelectTrip(a.Id);
        svc.AddStay("Tokyo", "Japan");

        svc.SelectTrip(b.Id);
        svc.AddStay("Osaka", "Japan");

        var tripA = repo.GetById(a.Id)!;
        var tripB = repo.GetById(b.Id)!;

        Assert.Single(tripA.Stays);
        Assert.Equal("Tokyo", tripA.Stays.Single().Place.City);

        Assert.Single(tripB.Stays);
        Assert.Equal("Osaka", tripB.Stays.Single().Place.City);
    }
}