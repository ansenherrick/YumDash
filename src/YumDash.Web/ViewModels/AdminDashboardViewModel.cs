namespace YumDash.Web.ViewModels;

public class AdminDashboardViewModel
{
    public decimal EstimatedWeeklyRevenue { get; set; }
    public int PendingReservations { get; set; }
    public IReadOnlyList<CategoryStatViewModel> PopularCategories { get; set; } = [];
    public IReadOnlyList<DayStatViewModel> BusiestDays { get; set; } = [];
}

public class CategoryStatViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DayStatViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}
