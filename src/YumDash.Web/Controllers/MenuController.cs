using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YumDash.Web.Data;
using YumDash.Web.ViewModels;

namespace YumDash.Web.Controllers;

public class MenuController : Controller
{
    private readonly AppDbContext _context;

    public MenuController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(new MenuFilterViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> Items()
    {
        var items = await _context.MenuItems
            .AsNoTracking()
            .Where(menuItem => menuItem.IsAvailable)
            .OrderBy(menuItem => menuItem.Category)
            .ThenBy(menuItem => menuItem.Name)
            .Select(menuItem => new
            {
                menuItem.Id,
                menuItem.Name,
                Category = menuItem.Category.ToString(),
                menuItem.Price,
                menuItem.Description,
                menuItem.Allergens
            })
            .ToListAsync();

        return Json(items);
    }
}
