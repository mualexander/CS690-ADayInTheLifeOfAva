using TravelPlanner.Core.Interfaces;
using TravelPlanner.Core.Models;

namespace TravelPlanner.Core.Services;

public class InMemoryTripContext : ITripContext
{
    private readonly ITripRepository _repository;
    private Trip? _activeTrip;

    public Trip? ActiveTrip => _activeTrip;

    public InMemoryTripContext(ITripRepository repository)
    {
        _repository = repository;
    }

    public void SetActiveTrip(Guid tripId)
    {
        var trip = _repository.GetById(tripId)
            ?? throw new InvalidOperationException("Trip not found.");

        _activeTrip = trip;
    }
}