using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class MainMenuFlow
{
    public static AppMode Handle(TripService svc, InMemoryTripContext ctx)
    {
        MenuRenderer.ShowMainMenu();
        var command = MainMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case MainMenuCommand.ListTrips:
                MenuRenderer.ShowTrips(svc.GetTrips());
                return AppMode.MainMenu;

            case MainMenuCommand.CreateTrip:
                ConsolePrompts.CreateTrip(svc);
                return AppMode.TripMenu;

            case MainMenuCommand.SelectTrip:
                ConsolePrompts.SelectTrip(svc);
                return AppMode.TripMenu;

            case MainMenuCommand.SeedTestData:
                ConsolePrompts.SeedDemoData(svc);
                return AppMode.TripMenu;

            case MainMenuCommand.Quit:
                MenuRenderer.ShowMessage("Bye.");
                Environment.Exit(0);
                return AppMode.MainMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.MainMenu;
        }
    }
}