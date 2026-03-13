
using System;
using System.Linq;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;
using Xunit;

namespace TravelPlanner.Core.Tests.Services;

public class FileTripRepositoryTests
{
    [Fact]
    public void Bookmarks_PersistAcrossFileRepositoryReload()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        try
        {
            var repo1 = new FileTripRepository(tempFile);
            var ctx1 = new InMemoryTripContext(repo1);
            var svc1 = new TripService(repo1, ctx1);

            var trip = svc1.CreateTrip("Japan 2026", 5000m);
            svc1.SelectTrip(trip.Id);
            svc1.AddStay("Tokyo", "Japan");

            var stayId = svc1.GetStays().Single().Id;
            svc1.AddBookmarkToStay(stayId, "Sushi Place", "https://example.com", "try omakase");

            var repo2 = new FileTripRepository(tempFile);
            var reloadedTrip = repo2.GetById(trip.Id);

            Assert.NotNull(reloadedTrip);
            var reloadedStay = reloadedTrip!.Stays.Single();
            var bookmark = reloadedStay.Bookmarks.Single();

            Assert.Equal("Sushi Place", bookmark.Title);
            Assert.Equal("https://example.com", bookmark.Url);
            Assert.Equal("try omakase", bookmark.Notes);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}





