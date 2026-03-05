using TravelPlanner.Core.Models;

namespace TravelPlanner.Core.Persistence;

public static class SnapshotMapper
{
    public static TripSnapshot ToSnapshot(Trip trip)
    {
        return new TripSnapshot(
            trip.Id,
            trip.Name,
            trip.TotalBudget,
            trip.CreatedAt,
            trip.Stays.Select(ToSnapshot).ToList()
        );
    }

    private static StaySnapshot ToSnapshot(Stay stay)
    {
        return new StaySnapshot(
            stay.Id,
            stay.Place.City,
            stay.Place.Country,
            stay.StartDate,
            stay.EndDate,
            stay.Expenses.Select(ToSnapshot).ToList()
        );
    }

    private static ExpenseSnapshot ToSnapshot(Expense expense)
    {
        return new ExpenseSnapshot(
            expense.Id,
            expense.Date,
            expense.Amount,
            expense.Category.ToString(),
            expense.Note
        );
    }

    public static Trip FromSnapshot(TripSnapshot snap)
    {
        // Create Trip using public constructor, then "hydrate" stays/expenses.
        // We need a way to set Id/CreatedAt, and to add Stay with specific Id, etc.
        // We'll do that via internal helper methods on domain types (see next section).

        var trip = Trip.Hydrate(snap.Id, snap.Name, snap.TotalBudget, snap.CreatedAt);

        foreach (var staySnap in snap.Stays)
        {
            var place = Place.Hydrate(Guid.NewGuid(), staySnap.City, staySnap.Country); // place id not critical yet
            var stay = Stay.Hydrate(staySnap.Id, place, staySnap.StartDate, staySnap.EndDate);

            foreach (var expSnap in staySnap.Expenses)
            {
                if (!Enum.TryParse<ExpenseCategory>(expSnap.Category, ignoreCase: true, out var cat))
                    cat = ExpenseCategory.Other;

                var exp = Expense.Hydrate(expSnap.Id, expSnap.Date, expSnap.Amount, cat, expSnap.Note);
                stay.HydrateAddExpense(exp);
            }

            trip.HydrateAddStay(stay);
        }

        return trip;
    }
}
