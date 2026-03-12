using System;
using System.Collections.Generic;
using System.Text;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Services;

namespace TravelPlanner.ConsoleApp;

public static class MenuRenderer
{
    public static void ShowHeader(Trip? activeTrip, StaySummary? activeStay = null)
    {
        System.Console.WriteLine("TravelPlanner");
        System.Console.WriteLine(new string('-', 50));

        if (activeTrip is null)
        {
            System.Console.WriteLine("Active trip: (none)");
        }
        else
        {
            System.Console.WriteLine($"Active trip: {activeTrip.Name}");
            System.Console.WriteLine($"Budget: {activeTrip.TotalBudget:0.00}");
            System.Console.WriteLine($"Spent:  {activeTrip.TotalSpent():0.00}");
            System.Console.WriteLine($"Left:   {activeTrip.RemainingBudget():0.00}");
        }

        if (activeStay is not null)
        {
            System.Console.WriteLine($"Active stay: {activeStay.DisplayKey}");
        }

        System.Console.WriteLine();
    }

    public static void ShowMainMenu()
    {
        System.Console.WriteLine("Main Menu");
        System.Console.WriteLine("L) List trips");
        System.Console.WriteLine("C) Create trip");
        System.Console.WriteLine("S) Select trip");
        System.Console.WriteLine("T) Seed test data");
        System.Console.WriteLine("Q) Quit");
        System.Console.Write("Choice: ");
    }

    public static void ShowTripMenu()
    {
        System.Console.WriteLine("Trip Menu");
        System.Console.WriteLine("V) View trip summary");
        System.Console.WriteLine("L) List stays");
        System.Console.WriteLine("A) Add stay");
        System.Console.WriteLine("S) Select stay");
        System.Console.WriteLine("R) Rename trip");
        System.Console.WriteLine("B) Update budget");
        System.Console.WriteLine("X) Archive trip");
        System.Console.WriteLine("Q) Back");
        System.Console.Write("Choice: ");
    }

    public static void ShowStayMenu()
    {
        System.Console.WriteLine("Stay Menu");
        System.Console.WriteLine("V) View stay details");
        System.Console.WriteLine("A) Add expense");
        System.Console.WriteLine("R) Remove expense");
        System.Console.WriteLine("P) Set place");
        System.Console.WriteLine("I) Set start date");
        System.Console.WriteLine("O) Set end data");
        System.Console.WriteLine("X) Delete stay");
        System.Console.WriteLine("Q) Back");
        System.Console.Write("Choice: ");
    }

    public static void ShowTrips(IReadOnlyList<TripSummary> trips)
    {
        if (trips.Count == 0)
        {
            System.Console.WriteLine("(no trips yet)");
            return;
        }

        for (int i = 0; i < trips.Count; i++)
        {
            var t = trips[i];
            System.Console.WriteLine(
                $"{i + 1}. {t.Name} | Budget {t.TotalBudget:0.00} | Spent {t.TotalSpent:0.00} | Left {t.RemainingBudget:0.00} | Stays {t.StayCount}"
            );
        }
    }

    public static void ShowStays(IReadOnlyList<StaySummary> stays)
    {
        if (stays.Count == 0)
        {
            System.Console.WriteLine("(no stays yet)");
            return;
        }

        for (int i = 0; i < stays.Count; i++)
        {
            var s = stays[i];
            System.Console.WriteLine($"{i + 1}. {s.DisplayKey} | Spent {s.TotalSpent:0.00}");
        }
    }

    public static void ShowTripSummary(Trip trip)
    {
        System.Console.WriteLine("Trip Summary");
        System.Console.WriteLine($"Name:   {trip.Name}");
        System.Console.WriteLine($"Budget: {trip.TotalBudget:0.00}");
        System.Console.WriteLine($"Spent:  {trip.TotalSpent():0.00}");
        System.Console.WriteLine($"Left:   {trip.RemainingBudget():0.00}");
        System.Console.WriteLine($"Stays:  {trip.Stays.Count}");
    }

    public static void ShowStayDetails(StaySummary stay)
    {
        System.Console.WriteLine("Stay Details");
        System.Console.WriteLine($"Stay:    {stay.DisplayKey}");
        System.Console.WriteLine($"City:    {stay.City}");
        System.Console.WriteLine($"Country: {stay.Country}");
        System.Console.WriteLine($"Spent:   {stay.TotalSpent:0.00}");

        if (stay.StartDate.HasValue && stay.EndDate.HasValue)
        {
            System.Console.WriteLine($"Dates:   {stay.StartDate:yyyy-MM-dd} to {stay.EndDate:yyyy-MM-dd}");
        }
    }

    public static void ShowError(string message)
    {
        System.Console.WriteLine($"ERROR: {message}");
    }

    public static void ShowMessage(string message)
    {
        System.Console.WriteLine(message);
    }

    public static void BlankLine()
    {
        System.Console.WriteLine();
    }
}