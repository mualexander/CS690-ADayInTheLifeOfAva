using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class LodgingOptionDetailMenuFlow
{
    public static AppMode Handle(
        TripService svc,
        StaySummary? activeStay,
        ref LodgingOptionSummary? activeLodgingOption)
    {
        if (activeStay is null)
        {
            MenuRenderer.ShowMessage("No active stay selected.");
            return AppMode.TripMenu;
        }

        if (activeLodgingOption is null)
        {
            MenuRenderer.ShowMessage("No active lodging option selected.");
            return AppMode.LodgingOptionMenu;
        }

        MenuRenderer.ShowLodgingOptionDetailMenu();
        var command = LodgingOptionDetailMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case LodgingOptionDetailMenuCommand.ViewDetails:
                MenuRenderer.ShowLodgingOptionDetails(activeLodgingOption);
                return AppMode.LodgingOptionDetailMenu;

            case LodgingOptionDetailMenuCommand.UpdatePrice:
                ConsolePrompts.UpdateLodgingOptionPrice(svc, activeStay, ref activeLodgingOption);
                return activeLodgingOption is null ? AppMode.FlightOptionMenu : AppMode.FlightOptionDetailMenu;

            case LodgingOptionDetailMenuCommand.UpdateUrl:
                ConsolePrompts.UpdateLodgingOptionUrl(svc, activeStay, ref activeLodgingOption);
                return activeLodgingOption is null ? AppMode.FlightOptionMenu : AppMode.FlightOptionDetailMenu;

            case LodgingOptionDetailMenuCommand.MarkSelected:
                ConsolePrompts.MarkLodgingOptionSelected(svc, activeStay, ref activeLodgingOption);
                return activeLodgingOption is null ? AppMode.LodgingOptionMenu : AppMode.LodgingOptionDetailMenu;

            case LodgingOptionDetailMenuCommand.MarkNotSelected:
                ConsolePrompts.MarkLodgingOptionNotSelected(svc, activeStay, ref activeLodgingOption);
                return activeLodgingOption is null ? AppMode.LodgingOptionMenu : AppMode.LodgingOptionDetailMenu;

            case LodgingOptionDetailMenuCommand.Back:
                activeLodgingOption = null;
                return AppMode.LodgingOptionMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.LodgingOptionDetailMenu;
        }
    }
}