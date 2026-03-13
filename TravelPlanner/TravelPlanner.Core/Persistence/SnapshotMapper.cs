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
            stay.Expenses.Select(ToSnapshot).ToList(),
            stay.Bookmarks.Select(ToSnapshot).ToList()
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

    private static BookmarkSnapshot ToSnapshot(Bookmark bookmark)
    {
        return new BookmarkSnapshot(
            bookmark.Id,
            bookmark.Title,
            bookmark.Url,
            bookmark.Notes,
            bookmark.CreatedAt
        );
    }

    public static Trip FromSnapshot(TripSnapshot snap)
    {
        var trip = Trip.Hydrate(snap.Id, snap.Name, snap.TotalBudget, snap.CreatedAt);

        foreach (var staySnap in snap.Stays)
        {
            var place = Place.Hydrate(Guid.NewGuid(), staySnap.City, staySnap.Country);
            var stay = Stay.Hydrate(staySnap.Id, place, staySnap.StartDate, staySnap.EndDate);

            foreach (var expSnap in staySnap.Expenses ?? new List<ExpenseSnapshot>())
            {
                if (!Enum.TryParse<ExpenseCategory>(expSnap.Category, ignoreCase: true, out var cat))
                    cat = ExpenseCategory.Other;

                var exp = Expense.Hydrate(expSnap.Id, expSnap.Date, expSnap.Amount, cat, expSnap.Note);
                stay.HydrateAddExpense(exp);
            }

            foreach(var bookmarkSnap in staySnap.Bookmarks ?? new List<BookmarkSnapshot>())
{
                var bookmark = Bookmark.Hydrate(
                    bookmarkSnap.Id,
                    bookmarkSnap.Title,
                    bookmarkSnap.Url,
                    bookmarkSnap.Notes,
                    bookmarkSnap.CreatedAt
                );

                stay.HydrateAddBookmark(bookmark);
            }

            trip.HydrateAddStay(stay);
        }

        return trip;
    }
}
