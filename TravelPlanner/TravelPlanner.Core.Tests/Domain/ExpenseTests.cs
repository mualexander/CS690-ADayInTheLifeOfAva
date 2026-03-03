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
            new Expense(DateTime.UtcNow.Date, 0m, ExpenseCategory.Food));

        Assert.Throws<ArgumentException>(() =>
            new Expense(DateTime.UtcNow.Date, -1m, ExpenseCategory.Food));
    }

    [Fact]
    public void Constructor_TrimsEmptyNoteToNull()
    {
        var e1 = new Expense(DateTime.UtcNow.Date, 10m, ExpenseCategory.Food, "   ");
        Assert.Null(e1.Note);

        var e2 = new Expense(DateTime.UtcNow.Date, 10m, ExpenseCategory.Food, " ramen ");
        Assert.Equal("ramen", e2.Note);
    }
}