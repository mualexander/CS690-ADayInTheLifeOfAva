using TravelPlanner.Cli;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;

const string DataPath = "data/trips.json";

var repo = new FileTripRepository(DataPath);
var ctx = new InMemoryTripContext(repo);
var svc = new TripService(repo, ctx);

var app = new ConsoleApplication(svc, ctx, DataPath);
app.Run();