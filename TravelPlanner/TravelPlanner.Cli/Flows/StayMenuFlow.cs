using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class StayMenuFlow
{
    public static AppMode Handle(TripService svc, ref StaySummary? activeStay)
    {
        if (activeStay is null)
        {
            MenuRenderer.ShowMessage("No active stay selected.");
            return AppMode.TripMenu;
        }

        MenuRenderer.ShowStayMenu();
        var command = StayMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case StayMenuCommand.ViewStayDetails:
                MenuRenderer.ShowStayDetails(activeStay);
                return AppMode.StayMenu;

            case StayMenuCommand.ManageExpenses:
                return AppMode.ExpenseMenu;

            case StayMenuCommand.ManageBookmarks:
                return AppMode.BookmarkMenu;

            case StayMenuCommand.ManageFlightOptions:
                return AppMode.FlightOptionMenu;

            case StayMenuCommand.SetPlace:
                ConsolePrompts.SetStayPlace(svc, ref activeStay);
                return AppMode.StayMenu;

            case StayMenuCommand.SetStartDate:
                ConsolePrompts.SetStayStartDate(svc, ref activeStay);
                return AppMode.StayMenu;

            case StayMenuCommand.SetEndDate:
                ConsolePrompts.SetStayEndDate(svc, ref activeStay);
                return AppMode.StayMenu;

            case StayMenuCommand.DeleteStay:
                ConsolePrompts.DeleteActiveStay(svc, ref activeStay);
                return AppMode.TripMenu;

            case StayMenuCommand.Back:
                activeStay = null;
                return AppMode.TripMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.StayMenu;
        }
    }
}