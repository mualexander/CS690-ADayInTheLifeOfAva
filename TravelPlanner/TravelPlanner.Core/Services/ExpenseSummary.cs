using System;
using System.Collections.Generic;
using System.Text;
using TravelPlanner.Core.Models;

namespace TravelPlanner.Core.Services;

public record ExpenseSummary(
    Guid Id,
    string Name,
    decimal Amount,
    ExpenseCategory Category,
    string? Notes,
    DateTime CreatedAt
);
