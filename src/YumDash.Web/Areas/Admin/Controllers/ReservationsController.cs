using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YumDash.Web.Data;
using YumDash.Web.Models;
using YumDash.Web.ViewModels;

namespace YumDash.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReservationsController : Controller
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var reservations = await _context.Reservations
            .AsNoTracking()
            .OrderByDescending(reservation => reservation.ReservationDate)
            .ToListAsync();

        var now = DateTime.Now;

        var pendingRequests = reservations
            .Where(reservation => reservation.Status == ReservationStatus.Pending)
            .OrderBy(reservation => reservation.ReservationDate)
            .ThenByDescending(reservation => reservation.CreatedAt)
            .ToList();

        var upcomingReservations = reservations
            .Where(reservation => reservation.Status == ReservationStatus.Confirmed && reservation.ReservationDate >= now)
            .OrderBy(reservation => reservation.ReservationDate)
            .ToList();

        var reservationHistory = reservations
            .Where(reservation =>
                reservation.Status == ReservationStatus.Canceled ||
                reservation.ReservationDate < now)
            .OrderByDescending(reservation => reservation.ReservationDate)
            .ThenByDescending(reservation => reservation.CreatedAt)
            .ToList();

        var viewModel = new AdminReservationsIndexViewModel
        {
            PendingRequests = pendingRequests,
            UpcomingReservations = upcomingReservations,
            ReservationHistory = reservationHistory,
            PendingCount = pendingRequests.Count,
            UpcomingCount = upcomingReservations.Count,
            HistoryCount = reservationHistory.Count,
            UpcomingRevenue = upcomingReservations.Sum(reservation => reservation.EstimatedRevenue)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ReservationStatus status)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation is null)
        {
            return NotFound();
        }

        reservation.Status = status;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<FileResult> ExportWeekly(DateTime? startDate = null)
    {
        var start = (startDate ?? DateTime.Today).Date;
        var end = start.AddDays(7);

        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.ReservationDate >= start && reservation.ReservationDate < end)
            .OrderBy(reservation => reservation.ReservationDate)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("GuestName,Email,ReservationDate,PartySize,Status,EstimatedRevenue");

        foreach (var reservation in reservations)
        {
            csv.AppendLine($"{Escape(reservation.GuestName)},{Escape(reservation.Email)},{reservation.ReservationDate:O},{reservation.PartySize},{reservation.Status},{reservation.EstimatedRevenue}");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"reservations-{start:yyyy-MM-dd}.csv");
    }

    private static string Escape(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
