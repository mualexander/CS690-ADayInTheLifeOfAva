using TravelPlanner.Core.Interfaces;
using TravelPlanner.Core.Models;

namespace TravelPlanner.Core.Repositories;

public class InMemoryTripRepository : ITripRepository
{
    private readonly Dictionary<Guid, Trip> _trips = new();

    public void Add(Trip trip)
    {
        if (trip == null) throw new ArgumentNullException(nameof(trip));
        if (_trips.ContainsKey(trip.Id)) throw new InvalidOperationException("Trip already exists.");

        _trips[trip.Id] = trip;
    }

    public Trip? GetById(Guid id)
    {
        _trips.TryGetValue(id, out var trip);
        return trip;
    }

    public IEnumerable<Trip> GetAll()
    {
        // Return a snapshot to avoid callers messing with enumeration while we modify storage
        return _trips.Values.ToList();
    }

    public void Update(Trip trip)
    {
        if (trip == null) throw new ArgumentNullException(nameof(trip));
        if (!_trips.ContainsKey(trip.Id)) throw new InvalidOperationException("Trip not found.");

        _trips[trip.Id] = trip;
    }

    public void Delete(Guid id)
    {
        _trips.Remove(id);
    }
}