using YumDash.Web.Models;

namespace YumDash.Web.ViewModels;

public sealed class AdminContactMessagesIndexViewModel
{
    public IReadOnlyList<ContactMessage> PendingMessages { get; init; } = [];

    public IReadOnlyList<ContactMessage> ResolvedMessages { get; init; } = [];

    public IReadOnlyList<ContactMessage> ArchivedMessages { get; init; } = [];

    public int PendingCount { get; init; }

    public int ResolvedCount { get; init; }

    public int ArchivedCount { get; init; }
}
