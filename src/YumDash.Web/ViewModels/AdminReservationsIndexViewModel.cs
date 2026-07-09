using YumDash.Web.Models;

namespace YumDash.Web.ViewModels;

public sealed class AdminReservationsIndexViewModel
{
    public IReadOnlyList<Reservation> PendingRequests { get; init; } = [];

    public IReadOnlyList<Reservation> UpcomingReservations { get; init; } = [];

    public IReadOnlyList<Reservation> ReservationHistory { get; init; } = [];

    public int PendingCount { get; init; }

    public int UpcomingCount { get; init; }

    public int HistoryCount { get; init; }

    public decimal UpcomingRevenue { get; init; }
}
