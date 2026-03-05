using System.Text.Json;
using TravelPlanner.Core.Interfaces;
using TravelPlanner.Core.Models;
using TravelPlanner.Core.Persistence;

namespace TravelPlanner.Core.Repositories;

public class FileTripRepository : ITripRepository
{
    private readonly string _filePath;
    private readonly Dictionary<Guid, Trip> _cache = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public FileTripRepository(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        LoadFromDisk();
    }

    public void Add(Trip trip)
    {
        if (trip == null) throw new ArgumentNullException(nameof(trip));
        if (_cache.ContainsKey(trip.Id)) throw new InvalidOperationException("Trip already exists.");

        _cache[trip.Id] = trip;
        SaveToDisk();
    }

    public Trip? GetById(Guid id)
    {
        _cache.TryGetValue(id, out var trip);
        return trip;
    }

    public IEnumerable<Trip> GetAll() => _cache.Values.ToList();

    public void Update(Trip trip)
    {
        if (trip == null) throw new ArgumentNullException(nameof(trip));
        if (!_cache.ContainsKey(trip.Id)) throw new InvalidOperationException("Trip not found.");

        _cache[trip.Id] = trip;
        SaveToDisk();
    }

    public void Delete(Guid id)
    {
        _cache.Remove(id);
        SaveToDisk();
    }

    private void LoadFromDisk()
    {
        _cache.Clear();

        if (!File.Exists(_filePath))
            return;

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
            return;

        var snaps = JsonSerializer.Deserialize<List<TripSnapshot>>(json, JsonOptions) ?? new();

        foreach (var snap in snaps)
        {
            var trip = SnapshotMapper.FromSnapshot(snap);
            _cache[trip.Id] = trip;
        }
    }

    private void SaveToDisk()
    {
        var snaps = _cache.Values
            .OrderBy(t => t.CreatedAt)
            .Select(SnapshotMapper.ToSnapshot)
            .ToList();

        var json = JsonSerializer.Serialize(snaps, JsonOptions);

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(_filePath, json);
    }
}