using TravelPlanner.Cli.Commands;
using TravelPlanner.Cli.Parsers;
using TravelPlanner.Core.Services;

namespace TravelPlanner.Cli;

public static class ExpenseMenuFlow
{
    public static AppMode Handle(TripService svc, StaySummary? activeStay, ref ExpenseSummary? activeExpense)
    {
        if (activeStay is null)
        {
            MenuRenderer.ShowMessage("No active stay selected.");
            return AppMode.TripMenu;
        }

        MenuRenderer.ShowExpenseMenu();
        var command = ExpenseMenuParser.Parse(Console.ReadLine());
        MenuRenderer.BlankLine();

        switch (command)
        {
            case ExpenseMenuCommand.ListExpenses:
                MenuRenderer.ShowExpenses(svc.GetExpensesForStay(activeStay.Id));
                return AppMode.ExpenseMenu;

            case ExpenseMenuCommand.AddExpense:
                ConsolePrompts.AddExpense(svc, activeStay);
                return AppMode.ExpenseMenu;

            case ExpenseMenuCommand.SelectExpense:
                activeExpense = ConsolePrompts.SelectExpense(svc, activeStay);
                return AppMode.ExpenseDetailMenu;

            case ExpenseMenuCommand.DeleteExpense:
                ConsolePrompts.DeleteExpense(svc, activeStay);
                activeExpense = null;
                return AppMode.ExpenseMenu;

            case ExpenseMenuCommand.Back:
                activeExpense = null;
                return AppMode.StayMenu;

            default:
                MenuRenderer.ShowMessage("Unknown choice.");
                return AppMode.ExpenseMenu;
        }
    }
}