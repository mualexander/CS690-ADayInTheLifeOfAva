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

        Assert.Equal(180m, svc.GetTripTotalPlannedCost());
        Assert.Equal(4820m, svc.GetTripRemainingBudget());

        var updated = repo.GetById(trip.Id)!;
        var tokyoStay = updated.Stays.Single();
        Assert.Single(tokyoStay.Expenses);
        Assert.Equal(180m, tokyoStay.TotalPlannedCost());
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
        Assert.Equal(200m, summary.TotalPlannedCost);
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

    [Fact]
    public void GetStays_IncludesSelectedTravelCostsInSummary()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddExpenseToStay(stayId, "Meals", 100m, ExpenseCategory.Food, null);
        svc.AddFlightOptionToStay(
            stayId,
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0),
            500m);

        // assuming you have some way to select/mark the flight option; if not, do it through domain/repo for now

        var stay = repo.GetById(trip.Id)!.Stays.Single();
        stay.FlightOptions.Single().Select();
        repo.Update(repo.GetById(trip.Id)!);

        var summary = svc.GetStays().Single();

        Assert.Equal(100m, summary.ExpenseTotal);
        Assert.Equal(500m, summary.SelectedFlightTotal);
        Assert.Equal(600m, summary.TotalPlannedCost);
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

    [Fact]
    public void FlightOptions_PersistAcrossFileRepositoryReload()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        try
        {
            var repo1 = new FileTripRepository(tempFile);
            var ctx1 = new InMemoryTripContext(repo1);
            var svc1 = new TripService(repo1, ctx1);

            var trip = svc1.CreateTrip("California Trip", 2000m);
            svc1.SelectTrip(trip.Id);
            svc1.AddStay("Santa Cruz", "USA");

            var stayId = svc1.GetStays().Single().Id;

            svc1.AddFlightOptionToStay(
                stayId,
                "https://example.com/flight",
                "LAX",
                "SJC",
                new DateTime(2026, 3, 10, 8, 0, 0),
                new DateTime(2026, 3, 10, 9, 30, 0));

            var repo2 = new FileTripRepository(tempFile);
            var reloadedTrip = repo2.GetById(trip.Id);

            Assert.NotNull(reloadedTrip);

            var reloadedStay = reloadedTrip!.Stays.Single();
            var option = reloadedStay.FlightOptions.Single();

            Assert.Equal("https://example.com/flight", option.Url);
            Assert.Equal("LAX", option.FromAirportCode);
            Assert.Equal("SJC", option.ToAirportCode);
            Assert.Equal(new DateTime(2026, 3, 10, 8, 0, 0), option.DepartTime);
            Assert.Equal(new DateTime(2026, 3, 10, 9, 30, 0), option.ArriveTime);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void UpdateFlightOptionPrice_ChangesPrice_AndUpdatesLastCheckedAt()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(
            stayId,
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0),
            null);

        var before = svc.GetFlightOptionsForStay(stayId).Single();
        Assert.Null(before.Price);
        Assert.Null(before.LastCheckedAt);

        svc.UpdateFlightOptionPrice(stayId, before.Id, 599.99m);

        var after = svc.GetFlightOptionsForStay(stayId).Single();
        Assert.Equal(599.99m, after.Price);
        Assert.NotNull(after.LastCheckedAt);
    }

    [Fact]
    public void SelectFlightOption_MarksFlightAsSelected()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(
            stayId,
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0),
            500m);

        var optionId = svc.GetFlightOptionsForStay(stayId).Single().Id;

        svc.SelectFlightOption(stayId, optionId);

        var updated = svc.GetFlightOptionsForStay(stayId).Single();
        Assert.True(updated.IsSelected);
    }

    [Fact]
    public void DeselectFlightOption_MarksFlightAsNotSelected()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(
            stayId,
            "https://example.com/flight",
            "SFO",
            "HND",
            new DateTime(2026, 1, 10, 8, 0, 0),
            new DateTime(2026, 1, 11, 12, 0, 0),
            500m);

        var optionId = svc.GetFlightOptionsForStay(stayId).Single().Id;

        svc.SelectFlightOption(stayId, optionId);
        svc.DeselectFlightOption(stayId, optionId);

        var updated = svc.GetFlightOptionsForStay(stayId).Single();
        Assert.False(updated.IsSelected);
    }
    #endregion

    #region LodgingOption Tests
    [Fact]
    public void AddLodgingOptionToStay_AddsLodgingOption()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Kyoto", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(
            stayId,
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        var options = svc.GetLodgingOptionsForStay(stayId);

        Assert.Single(options);
        Assert.Equal("Budget Inn", options[0].PropertyName);
        Assert.Equal(new DateTime(2026, 4, 10), options[0].CheckInDate);
        Assert.Equal(new DateTime(2026, 4, 14), options[0].CheckOutDate);
    }

    [Fact]
    public void GetLodgingOptionsForStay_ReturnsOptionsOrderedByCheckInDate()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Kyoto", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(
            stayId,
            "https://example.com/fancy",
            "Fancy Hotel",
            new DateTime(2026, 4, 12),
            new DateTime(2026, 4, 14));

        svc.AddLodgingOptionToStay(
            stayId,
            "https://example.com/budget",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 12));

        var options = svc.GetLodgingOptionsForStay(stayId);

        Assert.Equal(2, options.Count);
        Assert.Equal("Budget Inn", options[0].PropertyName);
        Assert.Equal("Fancy Hotel", options[1].PropertyName);
    }

    [Fact]
    public void DeleteLodgingOption_RemovesLodgingOption()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Kyoto", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(
            stayId,
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        var optionId = svc.GetLodgingOptionsForStay(stayId).Single().Id;

        svc.DeleteLodgingOption(stayId, optionId);

        Assert.Empty(svc.GetLodgingOptionsForStay(stayId));
    }

    [Fact]
    public void AddLodgingOptionToStay_ThrowsWhenNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() =>
            svc.AddLodgingOptionToStay(
                Guid.NewGuid(),
                "https://example.com/hotel",
                "Budget Inn",
                new DateTime(2026, 4, 10),
                new DateTime(2026, 4, 14)));
    }

    [Fact]
    public void GetLodgingOptionsForStay_ThrowsWhenStayNotFound()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        Assert.Throws<InvalidOperationException>(() =>
            svc.GetLodgingOptionsForStay(Guid.NewGuid()));
    }

    [Fact]
    public void SelectLodgingOption_MarksFlightAsSelected()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(
            stayId,
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        var optionId = svc.GetLodgingOptionsForStay(stayId).Single().Id;

        svc.SelectLodgingOption(stayId, optionId);

        var updated = svc.GetLodgingOptionsForStay(stayId).Single();
        Assert.True(updated.IsSelected);
    }

    [Fact]
    public void DeselectLodgingOption_MarksFlightAsNotSelected()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");

        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(
            stayId,
            "https://example.com/hotel",
            "Budget Inn",
            new DateTime(2026, 4, 10),
            new DateTime(2026, 4, 14));

        var optionId = svc.GetLodgingOptionsForStay(stayId).Single().Id;

        svc.SelectLodgingOption(stayId, optionId);
        svc.DeselectLodgingOption(stayId, optionId);

        var updated = svc.GetLodgingOptionsForStay(stayId).Single();
        Assert.False(updated.IsSelected);
    }
    #endregion

    #region UpdateTripBudget Tests

    [Fact]
    public void UpdateTripBudget_UpdatesActiveTripBudget()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.SelectTrip(trip.Id);

        svc.UpdateTripBudget(7500m);

        var summary = svc.GetTrips().Single();
        Assert.Equal(7500m, summary.TotalBudget);
    }

    [Fact]
    public void UpdateTripBudget_ThrowsWhenNoActiveTrip()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        Assert.Throws<InvalidOperationException>(() => svc.UpdateTripBudget(1000m));
    }

    #endregion

    #region CreateTrip / DeleteTrip Tests

    [Fact]
    public void CreateTrip_ThrowsOnDuplicateName()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Japan 2026", 5000m);

        Assert.Throws<InvalidOperationException>(() => svc.CreateTrip("Japan 2026", 3000m));
    }

    [Fact]
    public void CreateTrip_DuplicateCheck_IsCaseInsensitive()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Japan 2026", 5000m);

        Assert.Throws<InvalidOperationException>(() => svc.CreateTrip("japan 2026", 3000m));
    }

    [Fact]
    public void DeleteTrip_RemovesTripFromRepository()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Japan 2026", 5000m);
        svc.DeleteTrip(trip.Id);

        Assert.Empty(svc.GetTrips());
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public void GetExpensesForStay_ReturnsExpensesOrderedByName()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddExpenseToStay(stayId, "Taxis",    20m, ExpenseCategory.Transportation);
        svc.AddExpenseToStay(stayId, "Breakfast", 10m, ExpenseCategory.Food);
        svc.AddExpenseToStay(stayId, "Museum",    15m, ExpenseCategory.Activities);

        var names = svc.GetExpensesForStay(stayId).Select(e => e.Name).ToList();
        Assert.Equal(new[] { "Breakfast", "Museum", "Taxis" }, names);
    }

    [Fact]
    public void GetBookmarksForStay_ReturnsBookmarksOrderedByTitle()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddBookmarkToStay(stayId, "Tsukiji Market", "https://example.com/tsukiji");
        svc.AddBookmarkToStay(stayId, "Akihabara Guide", "https://example.com/akihabara");
        svc.AddBookmarkToStay(stayId, "Shinjuku Tips", "https://example.com/shinjuku");

        var titles = svc.GetBookmarksForStay(stayId).Select(b => b.Title).ToList();
        Assert.Equal(new[] { "Akihabara Guide", "Shinjuku Tips", "Tsukiji Market" }, titles);
    }

    [Fact]
    public void GetFlightOptionsForStay_ReturnsOptionsOrderedByDepartureTime()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(stayId, "https://example.com/b", "SFO", "HND",
            new DateTime(2026, 1, 15, 14, 0, 0), new DateTime(2026, 1, 16, 18, 0, 0));
        svc.AddFlightOptionToStay(stayId, "https://example.com/a", "LAX", "NRT",
            new DateTime(2026, 1, 10, 8, 0, 0),  new DateTime(2026, 1, 11, 12, 0, 0));

        var departures = svc.GetFlightOptionsForStay(stayId).Select(f => f.DepartTime).ToList();
        Assert.True(departures[0] < departures[1]);
    }

    #endregion

    #region UpdateFlightOptionUrl / UpdateLodgingOptionUrl Tests

    [Fact]
    public void UpdateFlightOptionUrl_ChangesUrl()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(stayId, "https://old.example", "SFO", "HND",
            new DateTime(2026, 1, 10, 8, 0, 0), new DateTime(2026, 1, 11, 12, 0, 0));
        var flightId = svc.GetFlightOptionsForStay(stayId).Single().Id;

        svc.UpdateFlightOptionUrl(stayId, flightId, "https://new.example");

        Assert.Equal("https://new.example", svc.GetFlightOptionsForStay(stayId).Single().Url);
    }

    [Fact]
    public void UpdateLodgingOptionUrl_ChangesUrl()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        var trip = svc.CreateTrip("Trip", 5000m);
        svc.SelectTrip(trip.Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(stayId, "https://old.example", "Hotel A",
            new DateTime(2026, 4, 10), new DateTime(2026, 4, 14));
        var lodgingId = svc.GetLodgingOptionsForStay(stayId).Single().Id;

        svc.UpdateLodgingOptionUrl(stayId, lodgingId, "https://new.example");

        Assert.Equal("https://new.example", svc.GetLodgingOptionsForStay(stayId).Single().Url);
    }

    #endregion

    #region LodgingOption Rating and Neighborhood Tests

    [Fact]
    public void AddLodgingOptionToStay_WithRatingAndNeighborhood_PersistsBothFields()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 0m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(stayId, "https://example.com", "Hotel A",
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 5),
            rating: 4.5m, neighborhood: "Shinjuku");

        var lodging = svc.GetLodgingOptionsForStay(stayId).Single();
        Assert.Equal(4.5m, lodging.Rating);
        Assert.Equal("Shinjuku", lodging.Neighborhood);
    }

    [Fact]
    public void AddLodgingOptionToStay_WithoutOptionalFields_LeavesThemNull()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 0m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(stayId, "https://example.com", "Hotel A",
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));

        var lodging = svc.GetLodgingOptionsForStay(stayId).Single();
        Assert.Null(lodging.Rating);
        Assert.Null(lodging.Neighborhood);
    }

    [Fact]
    public void UpdateLodgingOptionRating_ChangesRating()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 0m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(stayId, "https://example.com", "Hotel A",
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));
        var lodgingId = svc.GetLodgingOptionsForStay(stayId).Single().Id;

        svc.UpdateLodgingOptionRating(stayId, lodgingId, 3.5m);

        Assert.Equal(3.5m, svc.GetLodgingOptionsForStay(stayId).Single().Rating);
    }

    [Fact]
    public void UpdateLodgingOptionNeighborhood_ChangesNeighborhood()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 0m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(stayId, "https://example.com", "Hotel A",
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));
        var lodgingId = svc.GetLodgingOptionsForStay(stayId).Single().Id;

        svc.UpdateLodgingOptionNeighborhood(stayId, lodgingId, "Ginza");

        Assert.Equal("Ginza", svc.GetLodgingOptionsForStay(stayId).Single().Neighborhood);
    }

    #endregion

    #region StayStatus Tests

    [Fact]
    public void AddStay_DefaultStatus_IsIdea()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 0m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");

        Assert.Equal(StayStatus.Idea, svc.GetStays().Single().Status);
    }

    [Fact]
    public void AddStay_WithExplicitStatus_PersistsStatus()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 0m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan", status: StayStatus.Shortlist);

        Assert.Equal(StayStatus.Shortlist, svc.GetStays().Single().Status);
    }

    [Fact]
    public void SetStayStatus_UpdatesStatus()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 0m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.SetStayStatus(stayId, StayStatus.Locked);

        Assert.Equal(StayStatus.Locked, svc.GetStays().Single().Status);
    }

    [Fact]
    public void SetStayStatus_ThrowsForUnknownStay()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 0m);
        svc.SelectTrip(svc.GetTrips().Single().Id);

        Assert.Throws<InvalidOperationException>(() =>
            svc.SetStayStatus(Guid.NewGuid(), StayStatus.Locked));
    }

    #endregion

    #region IsOverBudget After Price Update Tests

    [Fact]
    public void IsOverBudget_ReturnsTrueAfterSelectedFlightPriceIncreasedPastBudget()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 500m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(stayId, "https://example.com", "SFO", "NRT",
            new DateTime(2026, 6, 1, 8, 0, 0), new DateTime(2026, 6, 2, 12, 0, 0), 400m);
        var flightId = svc.GetFlightOptionsForStay(stayId).Single().Id;
        svc.SelectFlightOption(stayId, flightId);

        Assert.False(svc.IsOverBudget());

        svc.UpdateFlightOptionPrice(stayId, flightId, 600m);

        Assert.True(svc.IsOverBudget());
    }

    [Fact]
    public void IsOverBudget_ReturnsFalseAfterUnselectedFlightPriceIncreasedPastBudget()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 500m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddFlightOptionToStay(stayId, "https://example.com", "SFO", "NRT",
            new DateTime(2026, 6, 1, 8, 0, 0), new DateTime(2026, 6, 2, 12, 0, 0), 400m);
        var flightId = svc.GetFlightOptionsForStay(stayId).Single().Id;
        // deliberately not selected

        svc.UpdateFlightOptionPrice(stayId, flightId, 600m);

        Assert.False(svc.IsOverBudget());
    }

    [Fact]
    public void IsOverBudget_ReturnsTrueAfterSelectedLodgingPriceIncreasedPastBudget()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 500m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(stayId, "https://example.com", "Hotel A",
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 5), 400m);
        var lodgingId = svc.GetLodgingOptionsForStay(stayId).Single().Id;
        svc.SelectLodgingOption(stayId, lodgingId);

        Assert.False(svc.IsOverBudget());

        svc.UpdateLodgingOptionPrice(stayId, lodgingId, 600m);

        Assert.True(svc.IsOverBudget());
    }

    [Fact]
    public void IsOverBudget_ReturnsFalseAfterUnselectedLodgingPriceIncreasedPastBudget()
    {
        var repo = new InMemoryTripRepository();
        var ctx = new InMemoryTripContext(repo);
        var svc = new TripService(repo, ctx);

        svc.CreateTrip("Trip", 500m);
        svc.SelectTrip(svc.GetTrips().Single().Id);
        svc.AddStay("Tokyo", "Japan");
        var stayId = svc.GetStays().Single().Id;

        svc.AddLodgingOptionToStay(stayId, "https://example.com", "Hotel A",
            new DateTime(2026, 6, 1), new DateTime(2026, 6, 5), 400m);
        var lodgingId = svc.GetLodgingOptionsForStay(stayId).Single().Id;
        // deliberately not selected

        svc.UpdateLodgingOptionPrice(stayId, lodgingId, 600m);

        Assert.False(svc.IsOverBudget());
    }

    #endregion
}