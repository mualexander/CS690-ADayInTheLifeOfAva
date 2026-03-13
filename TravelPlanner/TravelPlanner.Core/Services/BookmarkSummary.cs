using System;
using System.Collections.Generic;
using System.Text;

namespace TravelPlanner.Core.Services;

public record BookmarkSummary(
    Guid Id,
    string Title,
    string Url,
    string? Notes,
    DateTime CreatedAt
);
