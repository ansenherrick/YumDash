using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YumDash.Web.Data;
using YumDash.Web.Models;
using YumDash.Web.ViewModels;

namespace YumDash.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var weekEnd = today.AddDays(7);

        var upcomingReservations = await _context.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.ReservationDate >= today && reservation.ReservationDate < weekEnd)
            .ToListAsync();

        var popularCategories = await _context.MenuItems
            .AsNoTracking()
            .GroupBy(menuItem => menuItem.Category)
            .Select(group => new CategoryStatViewModel
            {
                Label = group.Key.ToString(),
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ToListAsync();

        var busiestDays = upcomingReservations
            .GroupBy(reservation => reservation.ReservationDate.DayOfWeek)
            .Select(group => new DayStatViewModel
            {
                Label = group.Key.ToString(),
                Count = group.Sum(reservation => reservation.PartySize)
            })
            .OrderByDescending(item => item.Count)
            .ToList();

        var model = new AdminDashboardViewModel
        {
            EstimatedWeeklyRevenue = upcomingReservations
                .Where(reservation => reservation.Status != ReservationStatus.Canceled)
                .Sum(reservation => reservation.EstimatedRevenue),
            PendingReservations = upcomingReservations.Count(reservation => reservation.Status == ReservationStatus.Pending),
            PopularCategories = popularCategories,
            BusiestDays = busiestDays
        };

        return View(model);
    }
}
