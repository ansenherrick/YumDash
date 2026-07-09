using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YumDash.Web.Data;
using YumDash.Web.Models;
using YumDash.Web.ViewModels;

namespace YumDash.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ContactController : Controller
{
    private readonly AppDbContext _context;

    public ContactController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var messages = await _context.ContactMessages
            .AsNoTracking()
            .OrderByDescending(message => message.CreatedAt)
            .ToListAsync();

        var pendingMessages = messages
            .Where(message => message.Status == SubmissionStatus.Pending)
            .OrderByDescending(message => message.CreatedAt)
            .ToList();

        var resolvedMessages = messages
            .Where(message => message.Status == SubmissionStatus.Resolved)
            .OrderByDescending(message => message.CreatedAt)
            .ToList();

        var archivedMessages = messages
            .Where(message => message.Status == SubmissionStatus.Archived)
            .OrderByDescending(message => message.CreatedAt)
            .ToList();

        var viewModel = new AdminContactMessagesIndexViewModel
        {
            PendingMessages = pendingMessages,
            ResolvedMessages = resolvedMessages,
            ArchivedMessages = archivedMessages,
            PendingCount = pendingMessages.Count,
            ResolvedCount = resolvedMessages.Count,
            ArchivedCount = archivedMessages.Count
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, SubmissionStatus status)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        message.Status = status;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
