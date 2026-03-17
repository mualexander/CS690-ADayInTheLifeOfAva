using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class FlightOptionDetailMenuFlow
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

        if (activeFlightOption is null)
        {
            MenuRenderer.ShowMessage("No active flight option selected.");
            return AppMode.FlightOptionMenu;
        }

        MenuRenderer.ShowFlightOptionDetailMenu();
        var command = FlightOptionDetailMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case FlightOptionDetailMenuCommand.ViewDetails:
                MenuRenderer.ShowFlightOptionDetails(activeFlightOption);
                return AppMode.FlightOptionDetailMenu;

            case FlightOptionDetailMenuCommand.UpdatePrice:
                ConsolePrompts.UpdateFlightOptionPrice(svc, activeStay, ref activeFlightOption);
                return activeFlightOption is null ? AppMode.FlightOptionMenu : AppMode.FlightOptionDetailMenu;

            case FlightOptionDetailMenuCommand.UpdateUrl:
                ConsolePrompts.UpdateFlightOptionUrl(svc, activeStay, ref activeFlightOption);
                return activeFlightOption is null ? AppMode.FlightOptionMenu : AppMode.FlightOptionDetailMenu;

            case FlightOptionDetailMenuCommand.Back:
                activeFlightOption = null;
                return AppMode.FlightOptionMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.FlightOptionDetailMenu;
        }
    }
}