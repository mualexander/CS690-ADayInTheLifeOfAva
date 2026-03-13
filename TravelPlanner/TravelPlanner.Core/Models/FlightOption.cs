namespace TravelPlanner.Core.Models;

public class FlightOption : TravelOption
{
    public string FromAirportCode { get; private set; }
    public string ToAirportCode { get; private set; }

    public DateTime DepartTime { get; private set; }
    public DateTime ArriveTime { get; private set; }

    public FlightOption(
        string url,
        string fromAirportCode,
        string toAirportCode,
        DateTime departTime,
        DateTime arriveTime)
        : base(url)
    {
        if (string.IsNullOrWhiteSpace(fromAirportCode))
            throw new ArgumentException("From airport code cannot be empty.", nameof(fromAirportCode));

        if (string.IsNullOrWhiteSpace(toAirportCode))
            throw new ArgumentException("To airport code cannot be empty.", nameof(toAirportCode));

        if (arriveTime < departTime)
            throw new ArgumentException("Arrival time cannot be before departure time.");

        FromAirportCode = NormalizeAirportCode(fromAirportCode);
        ToAirportCode = NormalizeAirportCode(toAirportCode);
        DepartTime = departTime;
        ArriveTime = arriveTime;
    }

    public void UpdateRoute(string fromAirportCode, string toAirportCode)
    {
        if (string.IsNullOrWhiteSpace(fromAirportCode))
            throw new ArgumentException("From airport code cannot be empty.", nameof(fromAirportCode));

        if (string.IsNullOrWhiteSpace(toAirportCode))
            throw new ArgumentException("To airport code cannot be empty.", nameof(toAirportCode));

        FromAirportCode = NormalizeAirportCode(fromAirportCode);
        ToAirportCode = NormalizeAirportCode(toAirportCode);
    }

    public void UpdateTimes(DateTime departTime, DateTime arriveTime)
    {
        if (arriveTime < departTime)
            throw new ArgumentException("Arrival time cannot be before departure time.");

        DepartTime = departTime;
        ArriveTime = arriveTime;
    }

    private static string NormalizeAirportCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    internal static FlightOption Hydrate(
    Guid id,
    string url,
    DateTime createdAt,
    DateTime? lastCheckedAt,
    bool isSelected,
    string fromAirportCode,
    string toAirportCode,
    DateTime departTime,
    DateTime arriveTime)
    {
        var option = new FlightOption(
            url,
            fromAirportCode,
            toAirportCode,
            departTime,
            arriveTime);

        option.Id = id;
        option.CreatedAt = createdAt;

        if (lastCheckedAt.HasValue)
        {
            option.MarkChecked(lastCheckedAt.Value);
        }

        if (isSelected)
        {
            option.Select();
        }

        return option;
    }
}