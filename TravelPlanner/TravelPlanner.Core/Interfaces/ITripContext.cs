using TravelPlanner.Core.Models;

namespace TravelPlanner.Core.Interfaces;

public interface ITripContext
{
	Trip? ActiveTrip { get; }
	void SetActiveTrip(Guid tripId);
}