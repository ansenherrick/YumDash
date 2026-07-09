using Microsoft.AspNetCore.Mvc;
using YumDash.Web.Data;
using YumDash.Web.Models;

namespace YumDash.Web.Controllers;

public class ContactController : Controller
{
    private readonly AppDbContext _context;

    public ContactController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new ContactMessage());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactMessage formModel)
    {
        if (!ModelState.IsValid)
        {
            return View(formModel);
        }

        formModel.CreatedAt = DateTime.UtcNow;
        formModel.Status = SubmissionStatus.Pending;
        _context.ContactMessages.Add(formModel);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Message sent successfully.";
        return RedirectToAction(nameof(Index));
    }
}
