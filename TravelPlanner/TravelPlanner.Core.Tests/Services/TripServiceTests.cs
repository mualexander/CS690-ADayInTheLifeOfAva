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
    public void GetStays_IncludesDisplayKey()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        var stays = svc.GetStays();
        Assert.Single(stays);
        Assert.Equal("Tokyo, Japan (2026-01-10..2026-01-14)", stays[0].DisplayKey);
    }

    [Fact]
    public void AddExpenseToStay_ThrowsIfNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() =>
            svc.AddExpenseToStay(Guid.NewGuid(), "Meals", 10m, ExpenseCategory.Food));
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
            svc.AddExpenseToStay(Guid.NewGuid(), "Meals", 10m, ExpenseCategory.Food));
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

        svc.AddExpenseToStay(stayId, "Meals", 180m, ExpenseCategory.Food, "Sushi");

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

        svc.AddExpenseToStay(stayId, "Meals", 200m, ExpenseCategory.Food);

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

    [Fact]
    public void GetStays_ReturnsDisplayKey()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        var stays = svc.GetStays();

        Assert.Single(stays);
        Assert.Equal("Tokyo, Japan (2026-01-10..2026-01-14)", stays[0].DisplayKey);
    }

    [Fact]
    public void ArchiveActiveTrip_ThrowsWhenNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() => svc.ArchiveActiveTrip());
    }

    [Fact]
    public void ArchiveActiveTrip_SetsTripArchived()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        svc.ArchiveActiveTrip();

        var updated = repo.GetById(trip.Id);
        Assert.NotNull(updated);
        Assert.True(updated!.IsArchived);
    }

    [Fact]
    public void GetTrips_DoesNotReturnArchivedTrips()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var activeTrip = svc.CreateTrip("Japan 2026", 5000m);
        var archivedTrip = svc.CreateTrip("Austin Weekend", 1200m);

        svc.SelectTrip(archivedTrip.Id);
        svc.ArchiveActiveTrip();

        var trips = svc.GetTrips();

        Assert.Contains(trips, t => t.Id == activeTrip.Id);
        Assert.DoesNotContain(trips, t => t.Id == archivedTrip.Id);
    }

    #region Stay Tests

    [Fact]
    public void UpdateStayPlace_ChangesPlace()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.UpdateStayPlace(stayId, "Osaka", "Japan");

        var updatedStay = svc.GetStays().Single();
        Assert.Equal("Osaka", updatedStay.City);
        Assert.Equal("Japan", updatedStay.Country);
        Assert.Equal("Osaka, Japan", updatedStay.DisplayKey);
    }

    [Fact]
    public void UpdateStayStartDate_ChangesStartDate()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        var stayId = svc.GetStays().Single().Id;

        svc.UpdateStayStartDate(stayId, new DateTime(2026, 1, 9));

        var updatedStay = svc.GetStays().Single();
        Assert.Equal(new DateTime(2026, 1, 9), updatedStay.StartDate);
        Assert.Equal(new DateTime(2026, 1, 14), updatedStay.EndDate);
        Assert.Equal("Tokyo, Japan (2026-01-09..2026-01-14)", updatedStay.DisplayKey);
    }

    [Fact]
    public void UpdateStayEndDate_ChangesEndDate()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan", new DateTime(2026, 1, 10), new DateTime(2026, 1, 14));

        var stayId = svc.GetStays().Single().Id;

        svc.UpdateStayEndDate(stayId, new DateTime(2026, 1, 15));

        var updatedStay = svc.GetStays().Single();
        Assert.Equal(new DateTime(2026, 1, 10), updatedStay.StartDate);
        Assert.Equal(new DateTime(2026, 1, 15), updatedStay.EndDate);
        Assert.Equal("Tokyo, Japan (2026-01-10..2026-01-15)", updatedStay.DisplayKey);
    }

    [Fact]
    public void DeleteStay_RemovesStayFromTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.DeleteStay(stayId);

        Assert.Empty(svc.GetStays());
    }

    [Fact]
    public void UpdateStayPlace_ThrowsWhenStayNotFound()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        Assert.Throws<InvalidOperationException>(() =>
            svc.UpdateStayPlace(Guid.NewGuid(), "Osaka", "Japan"));
    }

    [Fact]
    public void UpdateStayStartDate_ThrowsWhenNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() =>
            svc.UpdateStayStartDate(Guid.NewGuid(), new DateTime(2026, 1, 9)));
    }

    [Fact]
    public void DeleteStay_ThrowsWhenNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() =>
            svc.DeleteStay(Guid.NewGuid()));
    }

    #endregion

    #region Bookmark Tests
    [Fact]
    public void AddBookmarkToStay_AddsBookmark()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddBookmarkToStay(stayId, "Sushi Place", "https://example.com", "try omakase");

        var bookmarks = svc.GetBookmarksForStay(stayId);
        Assert.Single(bookmarks);
        Assert.Equal("Sushi Place", bookmarks[0].Title);
        Assert.Equal("https://example.com", bookmarks[0].Url);
        Assert.Equal("try omakase", bookmarks[0].Notes);
    }

    [Fact]
    public void UpdateBookmarkTitle_ChangesTitle()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;
        svc.AddBookmarkToStay(stayId, "Old Title", "https://example.com");

        var bookmarkId = svc.GetBookmarksForStay(stayId).Single().Id;

        svc.UpdateBookmarkTitle(stayId, bookmarkId, "New Title");

        var updated = svc.GetBookmarksForStay(stayId).Single();
        Assert.Equal("New Title", updated.Title);
    }

    [Fact]
    public void UpdateBookmarkUrl_ChangesUrl()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;
        svc.AddBookmarkToStay(stayId, "Sushi Place", "https://old.example");

        var bookmarkId = svc.GetBookmarksForStay(stayId).Single().Id;

        svc.UpdateBookmarkUrl(stayId, bookmarkId, "https://new.example");

        var updated = svc.GetBookmarksForStay(stayId).Single();
        Assert.Equal("https://new.example", updated.Url);
    }

    [Fact]
    public void UpdateBookmarkNotes_ChangesNotes()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;
        svc.AddBookmarkToStay(stayId, "Sushi Place", "https://example.com", "old notes");

        var bookmarkId = svc.GetBookmarksForStay(stayId).Single().Id;

        svc.UpdateBookmarkNotes(stayId, bookmarkId, "new notes");

        var updated = svc.GetBookmarksForStay(stayId).Single();
        Assert.Equal("new notes", updated.Notes);
    }

    [Fact]
    public void UpdateBookmarkNotes_CanClearNotes()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;
        svc.AddBookmarkToStay(stayId, "Sushi Place", "https://example.com", "old notes");

        var bookmarkId = svc.GetBookmarksForStay(stayId).Single().Id;

        svc.UpdateBookmarkNotes(stayId, bookmarkId, "");

        var updated = svc.GetBookmarksForStay(stayId).Single();
        Assert.Null(updated.Notes);
    }

    [Fact]
    public void DeleteBookmark_RemovesBookmark()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;
        svc.AddBookmarkToStay(stayId, "Sushi Place", "https://example.com");

        var bookmarkId = svc.GetBookmarksForStay(stayId).Single().Id;

        svc.DeleteBookmark(stayId, bookmarkId);

        Assert.Empty(svc.GetBookmarksForStay(stayId));
    }

    [Fact]
    public void AddBookmarkToStay_ThrowsWhenNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() =>
            svc.AddBookmarkToStay(Guid.NewGuid(), "Sushi Place", "https://example.com"));
    }

    [Fact]
    public void GetBookmarksForStay_ThrowsWhenStayNotFound()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        Assert.Throws<InvalidOperationException>(() =>
            svc.GetBookmarksForStay(Guid.NewGuid()));
    }

    [Fact]
    public void UpdateBookmarkTitle_ThrowsWhenBookmarkNotFound()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        Assert.Throws<InvalidOperationException>(() =>
            svc.UpdateBookmarkTitle(stayId, Guid.NewGuid(), "New Title"));
    }
    #endregion
    #region FlightOption Tests
    [Fact]
    public void AddFlightOptionToStay_AddsFlightOption()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("California Trip", 2000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Santa Cruz", "USA");

        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(
            stayId,
            "https://example.com/flight",
            "LAX",
            "SJC",
            new DateTime(2026, 3, 10, 8, 0, 0),
            new DateTime(2026, 3, 10, 9, 30, 0));

        var options = svc.GetFlightOptionsForStay(stayId);

        Assert.Single(options);
        Assert.Equal("LAX", options[0].FromAirportCode);
        Assert.Equal("SJC", options[0].ToAirportCode);
    }

    [Fact]
    public void DeleteFlightOption_RemovesFlightOption()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("California Trip", 2000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Santa Cruz", "USA");

        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(
            stayId,
            "https://example.com/flight",
            "LAX",
            "SJC",
            new DateTime(2026, 3, 10, 8, 0, 0),
            new DateTime(2026, 3, 10, 9, 30, 0));

        var optionId = svc.GetFlightOptionsForStay(stayId).Single().Id;

        svc.DeleteFlightOption(stayId, optionId);

        Assert.Empty(svc.GetFlightOptionsForStay(stayId));
    }
    #endregion
}