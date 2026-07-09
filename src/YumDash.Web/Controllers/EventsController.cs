using Microsoft.AspNetCore.Mvc;
using YumDash.Web.Data;
using YumDash.Web.Models;

namespace YumDash.Web.Controllers;

public class EventsController : Controller
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EventInquiry());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventInquiry inquiry)
    {
        if (!ModelState.IsValid)
        {
            return View(inquiry);
        }

        inquiry.CreatedAt = DateTime.UtcNow;
        inquiry.Status = SubmissionStatus.Pending;
        _context.EventInquiries.Add(inquiry);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Private event inquiry received.";
        return RedirectToAction(nameof(Create));
    }
}
