using System;
using TravelPlanner.Core.Models;
using Xunit;

namespace TravelPlanner.Core.Tests.Domain;

public class ExpenseTests
{
    [Fact]
    public void Constructor_AmountMustBePositive()
    {
        Assert.Throws<ArgumentException>(() =>
            new Expense("Meals", 0m, ExpenseCategory.Food));

        Assert.Throws<ArgumentException>(() =>
            new Expense("Meals", -1m, ExpenseCategory.Food));
    }

    [Fact]
    public void Constructor_TrimsEmptyNoteToNull()
    {
        var e1 = new Expense("Meals", 10m, ExpenseCategory.Food, "   ");
        Assert.Null(e1.Notes);

        var e2 = new Expense("More Meals", 10m, ExpenseCategory.Food, " ramen ");
        Assert.Equal("ramen", e2.Notes);
    }

    [Fact]
    public void Constructor_SetsFieldsCorrectly()
    {
        var e = new Expense("Shinkansen", 150m, ExpenseCategory.Transportation, "Tokyo to Osaka");

        Assert.NotEqual(Guid.Empty, e.Id);
        Assert.Equal("Shinkansen", e.Name);
        Assert.Equal(150m, e.Amount);
        Assert.Equal(ExpenseCategory.Transportation, e.Category);
        Assert.Equal("Tokyo to Osaka", e.Notes);
        Assert.True(e.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_RejectsBlankName()
    {
        Assert.Throws<ArgumentException>(() => new Expense("", 10m, ExpenseCategory.Food));
        Assert.Throws<ArgumentException>(() => new Expense("   ", 10m, ExpenseCategory.Food));
    }

    [Fact]
    public void Rename_ChangesName()
    {
        var e = new Expense("Meals", 10m, ExpenseCategory.Food);
        e.Rename("Dinner");
        Assert.Equal("Dinner", e.Name);
    }

    [Fact]
    public void Rename_RejectsBlankName()
    {
        var e = new Expense("Meals", 10m, ExpenseCategory.Food);
        Assert.Throws<ArgumentException>(() => e.Rename(""));
        Assert.Throws<ArgumentException>(() => e.Rename("   "));
    }

    [Fact]
    public void UpdateAmount_ChangesAmount()
    {
        var e = new Expense("Meals", 10m, ExpenseCategory.Food);
        e.UpdateAmount(25m);
        Assert.Equal(25m, e.Amount);
    }

    [Fact]
    public void UpdateAmount_RejectsNonPositive()
    {
        var e = new Expense("Meals", 10m, ExpenseCategory.Food);
        Assert.Throws<ArgumentException>(() => e.UpdateAmount(0m));
        Assert.Throws<ArgumentException>(() => e.UpdateAmount(-5m));
    }

    [Fact]
    public void UpdateNotes_ChangesNotes()
    {
        var e = new Expense("Meals", 10m, ExpenseCategory.Food);
        e.UpdateNotes("with drinks");
        Assert.Equal("with drinks", e.Notes);
    }

    [Fact]
    public void UpdateNotes_ClearsNotesWhenBlank()
    {
        var e = new Expense("Meals", 10m, ExpenseCategory.Food, "original");
        e.UpdateNotes("   ");
        Assert.Null(e.Notes);

        e.UpdateNotes(null);
        Assert.Null(e.Notes);
    }
}