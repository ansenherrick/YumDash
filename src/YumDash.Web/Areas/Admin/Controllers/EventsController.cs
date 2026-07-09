using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YumDash.Web.Data;
using YumDash.Web.Models;
using YumDash.Web.ViewModels;

namespace YumDash.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class EventsController : Controller
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var inquiries = await _context.EventInquiries
            .AsNoTracking()
            .OrderByDescending(inquiry => inquiry.CreatedAt)
            .ToListAsync();

        var pendingInquiries = inquiries
            .Where(inquiry => inquiry.Status == SubmissionStatus.Pending)
            .OrderBy(inquiry => inquiry.EventDate ?? DateTime.MaxValue)
            .ThenByDescending(inquiry => inquiry.CreatedAt)
            .ToList();

        var resolvedInquiries = inquiries
            .Where(inquiry => inquiry.Status == SubmissionStatus.Resolved)
            .OrderByDescending(inquiry => inquiry.CreatedAt)
            .ToList();

        var archivedInquiries = inquiries
            .Where(inquiry => inquiry.Status == SubmissionStatus.Archived)
            .OrderByDescending(inquiry => inquiry.CreatedAt)
            .ToList();

        var viewModel = new AdminEventInquiriesIndexViewModel
        {
            PendingInquiries = pendingInquiries,
            ResolvedInquiries = resolvedInquiries,
            ArchivedInquiries = archivedInquiries,
            PendingCount = pendingInquiries.Count,
            ResolvedCount = resolvedInquiries.Count,
            ArchivedCount = archivedInquiries.Count,
            TotalPendingGuests = pendingInquiries.Sum(inquiry => inquiry.PartySize)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, SubmissionStatus status)
    {
        var inquiry = await _context.EventInquiries.FindAsync(id);
        if (inquiry is null)
        {
            return NotFound();
        }

        inquiry.Status = status;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
