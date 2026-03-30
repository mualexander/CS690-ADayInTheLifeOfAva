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
        var baseDate = new DateTime(2026, 1, 1);

        for (int i = 0; i < 100; i++)
        {
            var stay = trip.AddStay($"City{i}", "Testland");

            stay.AddExpense($"Expense{i}", 10.00m, ExpenseCategory.Other);

            for (int f = 0; f < 10; f++)
            {
                var flight = stay.AddFlightOption(
                    url: $"https://flights.example.com/{i}/{f}",
                    fromAirportCode: "AAA",
                    toAirportCode: "BBB",
                    departTime: baseDate.AddDays(i),
                    arriveTime: baseDate.AddDays(i).AddHours(5),
                    price: 5.00m);
                flight.Select();
            }

            for (int l = 0; l < 10; l++)
            {
                var lodging = stay.AddLodgingOption(
                    url: $"https://lodging.example.com/{i}/{l}",
                    propertyName: $"Hotel{i}_{l}",
                    checkInDate: baseDate.AddDays(i),
                    checkOutDate: baseDate.AddDays(i + 1),
                    price: 8.00m);
                lodging.Select();
            }
        }

        // 100 stays × ($10 expense + 10×$5 flights + 10×$8 lodging) = 100 × $140 = $14,000
        var sw = Stopwatch.StartNew();
        var cost = trip.TotalPlannedCost();
        sw.Stop();

        Assert.Equal(14_000.00m, cost);
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"TotalPlannedCost took {sw.ElapsedMilliseconds}ms, expected under 100ms.");
    }
}
