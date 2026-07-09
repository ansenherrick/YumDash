using Microsoft.AspNetCore.Mvc;
using YumDash.Web.Data;
using YumDash.Web.Models;

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
        return View(new Reservation
        {
            ReservationDate = DateTime.Today.AddDays(1).AddHours(19)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reservation reservation)
    {
        if (!ModelState.IsValid)
        {
            return View(reservation);
        }

        reservation.Status = ReservationStatus.Pending;
        reservation.CreatedAt = DateTime.UtcNow;

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reservation request sent. We'll follow up soon.";
        return RedirectToAction(nameof(Create));
    }
}
