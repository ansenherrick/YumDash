using YumDash.Web.Models;

namespace YumDash.Web.ViewModels;

public sealed class AdminEventInquiriesIndexViewModel
{
    public IReadOnlyList<EventInquiry> PendingInquiries { get; init; } = [];

    public IReadOnlyList<EventInquiry> ResolvedInquiries { get; init; } = [];

    public IReadOnlyList<EventInquiry> ArchivedInquiries { get; init; } = [];

    public int PendingCount { get; init; }

    public int ResolvedCount { get; init; }

    public int ArchivedCount { get; init; }

    public int TotalPendingGuests { get; init; }
}
