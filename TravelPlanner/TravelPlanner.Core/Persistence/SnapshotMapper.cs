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
        ) { WarnOnOverBudget = trip.WarnOnOverBudget };
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
            stay.Bookmarks.Select(ToSnapshot).ToList(),
            stay.FlightOptions.Select(ToSnapshot).ToList(),
            stay.LodgingOptions.Select(ToSnapshot).ToList()
        );
    }

    private static ExpenseSnapshot ToSnapshot(Expense expense)
    {
        return new ExpenseSnapshot(
            expense.Id,
            expense.Name,
            expense.Amount,
            expense.Category.ToString(),
            expense.Notes,
            expense.CreatedAt
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
        ) { Tags = bookmark.Tags.ToList() };
    }

    private static FlightOptionSnapshot ToSnapshot(FlightOption option)
    {
        return new FlightOptionSnapshot(
            option.Id,
            option.Url,
            option.Price,
            option.CreatedAt,
            option.LastCheckedAt,
            option.IsSelected,
            option.FromAirportCode,
            option.ToAirportCode,
            option.DepartTime,
            option.ArriveTime
        );
    }

    private static LodgingOptionSnapshot ToSnapshot(LodgingOption option)
    {
        return new LodgingOptionSnapshot(
            option.Id,
            option.Url,
            option.Price,
            option.CreatedAt,
            option.LastCheckedAt,
            option.IsSelected,
            option.PropertyName,
            option.CheckInDate,
            option.CheckOutDate
        );
    }

    public static Trip FromSnapshot(TripSnapshot snap)
    {
        var warnOnOverBudget = snap.WarnOnOverBudget ?? (snap.TotalBudget > 0);
        var trip = Trip.Hydrate(snap.Id, snap.Name, snap.TotalBudget, snap.CreatedAt, warnOnOverBudget);

        foreach (var staySnap in snap.Stays)
        {
            var place = Place.Hydrate(Guid.NewGuid(), staySnap.City, staySnap.Country);
            var stay = Stay.Hydrate(staySnap.Id, place, staySnap.StartDate, staySnap.EndDate);

            foreach (var expSnap in staySnap.Expenses ?? new List<ExpenseSnapshot>())
            {
                if (!Enum.TryParse<ExpenseCategory>(expSnap.Category, ignoreCase: true, out var cat))
                    cat = ExpenseCategory.Other;

                var exp = Expense.Hydrate(
                    expSnap.Id,
                    expSnap.Name,
                    expSnap.Amount,
                    cat,
                    expSnap.Note,
                    expSnap.CreatedAt
                 );
                stay.HydrateAddExpense(exp);
            }

            foreach(var bookmarkSnap in staySnap.Bookmarks ?? new List<BookmarkSnapshot>())
{
                var bookmark = Bookmark.Hydrate(
                    bookmarkSnap.Id,
                    bookmarkSnap.Title,
                    bookmarkSnap.Url,
                    bookmarkSnap.Notes,
                    bookmarkSnap.CreatedAt,
                    bookmarkSnap.Tags
                );

                stay.HydrateAddBookmark(bookmark);
            }

            foreach (var flightSnap in staySnap.FlightOptions ?? new List<FlightOptionSnapshot>())
            {
                var flight = FlightOption.Hydrate(
                    flightSnap.Id,
                    flightSnap.Url,
                    flightSnap.Price,
                    flightSnap.CreatedAt,
                    flightSnap.LastCheckedAt,
                    flightSnap.IsSelected,
                    flightSnap.FromAirportCode,
                    flightSnap.ToAirportCode,
                    flightSnap.DepartTime,
                    flightSnap.ArriveTime
                );

                stay.HydrateAddFlightOption(flight);
            }

            foreach (var lodgingSnap in staySnap.LodgingOptions ?? new List<LodgingOptionSnapshot>())
            {
                var lodging = LodgingOption.Hydrate(
                    lodgingSnap.Id,
                    lodgingSnap.Url,
                    lodgingSnap.Price,
                    lodgingSnap.CreatedAt,
                    lodgingSnap.LastCheckedAt,
                    lodgingSnap.IsSelected,
                    lodgingSnap.PropertyName,
                    lodgingSnap.CheckInDate,
                    lodgingSnap.CheckOutDate
                );

                stay.HydrateAddLodgingOption(lodging);
            }

            trip.HydrateAddStay(stay);
        }

        return trip;
    }
}
