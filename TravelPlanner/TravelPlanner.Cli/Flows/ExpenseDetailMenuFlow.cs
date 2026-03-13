using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class ExpenseDetailMenuFlow
{
    public static AppMode Handle(
        TripService svc,
        StaySummary? activeStay,
        ref ExpenseSummary? activeExpense)
    {
        if (activeStay is null)
        {
            MenuRenderer.ShowMessage("No active stay selected.");
            return AppMode.TripMenu;
        }

        if (activeExpense is null)
        {
            MenuRenderer.ShowMessage("No active expense selected.");
            return AppMode.ExpenseMenu;
        }

        MenuRenderer.ShowExpenseDetailMenu();
        var command = ExpenseDetailMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case ExpenseDetailMenuCommand.ViewDetails:
                MenuRenderer.ShowExpenseDetails(activeExpense);
                return AppMode.ExpenseDetailMenu;

            case ExpenseDetailMenuCommand.Rename:
                ConsolePrompts.RenameExpense(svc, activeStay, ref activeExpense);
                return activeExpense is null ? AppMode.ExpenseMenu : AppMode.ExpenseDetailMenu;

            case ExpenseDetailMenuCommand.UpdateAmount:
                ConsolePrompts.UpdateExpenseAmount(svc, activeStay, ref activeExpense);
                return activeExpense is null ? AppMode.ExpenseMenu : AppMode.ExpenseDetailMenu;

            case ExpenseDetailMenuCommand.UpdateNotes:
                ConsolePrompts.UpdateExpenseNotes(svc, activeStay, ref activeExpense);
                return activeExpense is null ? AppMode.ExpenseMenu : AppMode.ExpenseDetailMenu;

            case ExpenseDetailMenuCommand.Delete:
                ConsolePrompts.DeleteActiveExpense(svc, activeStay, ref activeExpense);
                return AppMode.ExpenseMenu;

            case ExpenseDetailMenuCommand.Back:
                activeExpense = null;
                return AppMode.ExpenseMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.ExpenseDetailMenu;
        }
    }
}