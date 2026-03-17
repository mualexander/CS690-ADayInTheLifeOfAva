using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class LodgingOptionMenuFlow
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

        MenuRenderer.ShowLodgingOptionMenu();
        var command = LodgingOptionMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case LodgingOptionMenuCommand.ListLodgingOptions:
                MenuRenderer.ShowLodgingOptions(svc.GetLodgingOptionsForStay(activeStay.Id));
                return AppMode.LodgingOptionMenu;

            case LodgingOptionMenuCommand.AddLodgingOption:
                ConsolePrompts.AddLodgingOption(svc, activeStay);
                return AppMode.LodgingOptionMenu;

            case LodgingOptionMenuCommand.SelectLodgingOption:
                activeLodgingOption = ConsolePrompts.SelectLodgingOption(svc, activeStay);
                return AppMode.LodgingOptionDetailMenu;

            case LodgingOptionMenuCommand.DeleteLodgingOption:
                ConsolePrompts.DeleteLodgingOption(svc, activeStay);
                activeLodgingOption = null;
                return AppMode.LodgingOptionMenu;

            case LodgingOptionMenuCommand.Back:
                activeLodgingOption = null;
                return AppMode.StayMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.LodgingOptionMenu;
        }
    }
}