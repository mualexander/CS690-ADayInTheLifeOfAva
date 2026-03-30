using System;
using System.Diagnostics;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class TripPerformanceTests
{
    [Fact]
    public void TotalPlannedCost_100StaysEachWithOneExpense_CompletesUnder100ms()
    {
        var trip = new Trip("Performance Test Trip", 100_000m);

        for (int i = 0; i < 100; i++)
        {
            var stay = trip.AddStay($"City{i}", "Testland");
            stay.AddExpense($"Expense{i}", 10.00m, ExpenseCategory.Other);
        }

        var sw = Stopwatch.StartNew();
        var cost = trip.TotalPlannedCost();
        sw.Stop();

        Assert.Equal(1000.00m, cost);
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"TotalPlannedCost took {sw.ElapsedMilliseconds}ms, expected under 100ms.");
    }
}
