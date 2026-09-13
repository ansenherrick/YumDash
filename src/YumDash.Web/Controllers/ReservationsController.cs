using Microsoft.AspNetCore.Mvc;
using YumDash.Web.Data;
using YumDash.Web.Models;
using YumDash.Web.ViewModels;

namespace YumDash.Web.Controllers;

public class ReservationsController : Controller
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ReservationRequestViewModel
        {
            ReservationDate = DateTime.Today.AddDays(1).AddHours(19)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // datetime-local submits a server-local wall time without a UTC kind.
        // PostgreSQL's timestamp-with-time-zone column requires a UTC instant.
        var reservation = new Reservation
        {
            GuestName = model.GuestName,
            Email = model.Email,
            Phone = model.Phone,
            ReservationDate = model.ReservationDate!.Value.ToUniversalTime(),
            PartySize = model.PartySize,
            EstimatedSpendPerGuest = model.EstimatedSpendPerGuest,
            Notes = model.Notes ?? string.Empty,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reservation request sent. We'll follow up soon.";
        return RedirectToAction(nameof(Create));
    }
}
