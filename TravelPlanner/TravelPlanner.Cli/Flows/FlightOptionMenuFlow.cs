using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class FlightOptionMenuFlow
{
    public static AppMode Handle(
        TripService svc,
        StaySummary? activeStay,
        ref FlightOptionSummary? activeFlightOption)
    {
        if (activeStay is null)
        {
            MenuRenderer.ShowMessage("No active stay selected.");
            return AppMode.TripMenu;
        }

        MenuRenderer.ShowFlightOptionMenu();
        var command = FlightOptionMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case FlightOptionMenuCommand.ListFlightOptions:
                MenuRenderer.ShowFlightOptions(svc.GetFlightOptionsForStay(activeStay.Id));
                return AppMode.FlightOptionMenu;

            case FlightOptionMenuCommand.AddFlightOption:
                ConsolePrompts.AddFlightOption(svc, activeStay);
                return AppMode.FlightOptionMenu;

            case FlightOptionMenuCommand.SelectFlightOption:
                activeFlightOption = ConsolePrompts.SelectFlightOption(svc, activeStay);
                return AppMode.FlightOptionDetailMenu;

            case FlightOptionMenuCommand.DeleteFlightOption:
                ConsolePrompts.DeleteFlightOption(svc, activeStay);
                activeFlightOption = null;
                return AppMode.FlightOptionMenu;

            case FlightOptionMenuCommand.Back:
                activeFlightOption = null;
                return AppMode.StayMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.FlightOptionMenu;
        }
    }
}