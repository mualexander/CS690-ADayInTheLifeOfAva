using static System.Net.WebRequestMethods;

namespace TravelPlanner.Core.Models;

public class Expense
{
    public Guid Id { get; internal set; }
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; internal set; }

    public Expense(string name, decimal amount, ExpenseCategory category, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Expense name cannot be empty.", nameof(name));
        if (amount <= 0) throw new ArgumentException("Expense amount must be > 0.", nameof(amount));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Amount = amount;
        Category = category;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Expense name cannot be empty.", nameof(newName));

        Name = newName.Trim();
    }

    public void UpdateAmount(decimal newAmount)
    {
        if (newAmount <= 0) throw new ArgumentException("Expense amount must be > 0.", nameof(newAmount));
        Amount = newAmount;
    }

    public void UpdateNotes(string? newNotes)
    {
        Notes = string.IsNullOrWhiteSpace(newNotes) ? null : newNotes.Trim();
    }

    public override string ToString()
    {
        return $"{Name}: ${Amount}";
    }


    internal static Expense Hydrate(Guid id, string name, decimal amount, ExpenseCategory category, string? notes, DateTime createdAt)
    {
        var e = new Expense(name, amount, category, notes);
        e.Id = id;
        e.CreatedAt = createdAt;
        return e;
    }
}