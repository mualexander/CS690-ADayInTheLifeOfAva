using Terminal.Gui;
using TravelPlanner.Cli.Views;
using TravelPlanner.Core.Repositories;
using TravelPlanner.Core.Services;

const string DataPath = "data/trips.json";

var repo = new FileTripRepository(DataPath);
var ctx  = new InMemoryTripContext(repo);
var svc  = new TripService(repo, ctx);

Application.Init();
try
{
    Application.Run(new MainView(svc, ctx));
}
finally
{
    Application.Shutdown();
}
