using YumDash.Web.Models;

namespace YumDash.Web.ViewModels;

public sealed class AdminMenuItemsIndexViewModel
{
    public string Sort { get; init; } = "category-dining";

    public MenuCategory? Category { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public string Availability { get; init; } = "all";

    public IReadOnlyList<AdminMenuItemRowViewModel> Items { get; init; } = [];

    public IReadOnlyList<MenuCategory> Categories { get; init; } = Enum.GetValues<MenuCategory>();

    public int PendingAdditionCount { get; init; }

    public int PendingDeletionCount { get; init; }

    public bool HasPendingChanges => PendingAdditionCount > 0 || PendingDeletionCount > 0;

    public bool HasActiveFilters =>
        Category.HasValue ||
        MinPrice.HasValue ||
        MaxPrice.HasValue ||
        !string.Equals(Availability, "all", StringComparison.OrdinalIgnoreCase);
}

public sealed class AdminMenuItemRowViewModel
{
    public int? Id { get; init; }

    public string? DraftId { get; init; }

    public string Name { get; init; } = string.Empty;

    public MenuCategory Category { get; init; }

    public decimal Price { get; init; }

    public bool IsAvailable { get; init; }

    public bool IsPendingAddition { get; init; }

    public bool IsPendingDeletion { get; init; }
}
