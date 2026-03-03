using System;
using System.Linq;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class TripTests
{
    [Fact]
    public void Rename_RejectsBlank()
    {
        var trip = new Trip("Japan", 1000m);
        Assert.Throws<ArgumentException>(() => trip.Rename(""));
        Assert.Throws<ArgumentException>(() => trip.Rename("   "));
    }

    [Fact]
    public void UpdateBudget_RejectsNegative()
    {
        var trip = new Trip("Japan", 1000m);
        Assert.Throws<ArgumentException>(() => trip.UpdateBudget(-1m));
    }

    [Fact]
    public void AddLocation_RejectsDuplicateByName()
    {
        var trip = new Trip("Japan", 1000m);
        trip.AddLocation(new Location("Tokyo", "Japan"));

        Assert.Throws<InvalidOperationException>(() =>
            trip.AddLocation(new Location("Tokyo", "Japan")));
    }

    [Fact]
    public void RemoveLocation_ThrowsWhenMissing()
    {
        var trip = new Trip("Japan", 1000m);
        Assert.Throws<InvalidOperationException>(() => trip.RemoveLocation(Guid.NewGuid()));
    }

    [Fact]
    public void TripTotals_RollUpAcrossLocations()
    {
        var trip = new Trip("Japan", 1000m);

        var tokyo = new Location("Tokyo", "Japan");
        tokyo.AddExpense(DateTime.UtcNow.Date, 100m, ExpenseCategory.Food);

        var osaka = new Location("Osaka", "Japan");
        osaka.AddExpense(DateTime.UtcNow.Date, 50m, ExpenseCategory.Activities);

        trip.AddLocation(tokyo);
        trip.AddLocation(osaka);

        Assert.Equal(150m, trip.TotalSpent());
        Assert.Equal(850m, trip.RemainingBudget());
    }
}