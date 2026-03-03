using TravelPlanner.Core.Models;

namespace TravelPlanner.Core.Interfaces;

public interface ITripRepository
{
    void Add(Trip trip);
    Trip? GetById(Guid id);
    IEnumerable<Trip> GetAll();
    void Update(Trip trip);
    void Delete(Guid id);
}