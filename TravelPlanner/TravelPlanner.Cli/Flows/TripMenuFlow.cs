using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class TripMenuFlow
{
    public static AppMode Handle(TripService svc, InMemoryTripContext ctx, ref StaySummary? activeStay)
    {
        if (ctx.ActiveTrip is null)
        {
            MenuRenderer.ShowMessage("No active trip selected.");
            return AppMode.MainMenu;
        }

        MenuRenderer.ShowTripMenu();
        var command = TripMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case TripMenuCommand.ListStays:
                MenuRenderer.ShowStays(svc.GetStays());
                return AppMode.TripMenu;

            case TripMenuCommand.AddStay:
                ConsolePrompts.AddStay(svc);
                return AppMode.TripMenu;

            case TripMenuCommand.SelectStay:
                activeStay = ConsolePrompts.SelectStay(svc);
                return AppMode.StayMenu;

            case TripMenuCommand.Back:
                activeStay = null;
                return AppMode.MainMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.TripMenu;
        }
    }
}