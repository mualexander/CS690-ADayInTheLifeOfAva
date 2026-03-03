namespace TravelPlanner.Core.Models;

public class Expense
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public string? Note { get; private set; }

    public Expense(DateTime date, decimal amount, ExpenseCategory category, string? note = null)
    {
        if (amount <= 0) throw new ArgumentException("Expense amount must be > 0.", nameof(amount));

        Id = Guid.NewGuid();
        Date = date;
        Amount = amount;
        Category = category;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}