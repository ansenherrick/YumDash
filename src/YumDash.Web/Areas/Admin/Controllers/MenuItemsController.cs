using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using YumDash.Web.Data;
using YumDash.Web.Models;
using YumDash.Web.ViewModels;

namespace YumDash.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MenuItemsController : Controller
{
    private const string PendingChangesTempDataKey = "AdminMenuPendingChanges";
    private readonly AppDbContext _context;

    public MenuItemsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string sort = "category-dining",
        MenuCategory? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string availability = "all")
    {
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
        {
            (minPrice, maxPrice) = (maxPrice, minPrice);
        }

        var pendingChanges = GetPendingChanges();
        var persistedItems = await _context.MenuItems.AsNoTracking().ToListAsync();

        var rows = persistedItems
            .Select(menuItem => new AdminMenuItemRowViewModel
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Category = menuItem.Category,
                Price = menuItem.Price,
                IsAvailable = menuItem.IsAvailable,
                IsPendingDeletion = pendingChanges.DeletedItemIds.Contains(menuItem.Id)
            })
            .ToList();

        rows.AddRange(pendingChanges.AddedItems.Select(addedItem => new AdminMenuItemRowViewModel
        {
            DraftId = addedItem.DraftId,
            Name = addedItem.Name,
            Category = addedItem.Category,
            Price = addedItem.Price,
            IsAvailable = addedItem.IsAvailable,
            IsPendingAddition = true
        }));

        IEnumerable<AdminMenuItemRowViewModel> filteredRows = rows;

        if (category.HasValue)
        {
            filteredRows = filteredRows.Where(menuItem => menuItem.Category == category.Value);
        }

        if (minPrice.HasValue)
        {
            filteredRows = filteredRows.Where(menuItem => menuItem.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            filteredRows = filteredRows.Where(menuItem => menuItem.Price <= maxPrice.Value);
        }

        filteredRows = availability.ToLowerInvariant() switch
        {
            "available" => filteredRows.Where(menuItem => menuItem.IsAvailable),
            "unavailable" => filteredRows.Where(menuItem => !menuItem.IsAvailable),
            _ => filteredRows
        };

        filteredRows = sort.ToLowerInvariant() switch
        {
            "name-asc" => filteredRows
                .OrderBy(menuItem => menuItem.Name),
            "name-desc" => filteredRows
                .OrderByDescending(menuItem => menuItem.Name),
            "category-alpha" => filteredRows
                .OrderBy(menuItem => menuItem.Category.ToString())
                .ThenBy(menuItem => menuItem.Name),
            "price-asc" => filteredRows
                .OrderBy(menuItem => menuItem.Price)
                .ThenBy(menuItem => menuItem.Name),
            "price-desc" => filteredRows
                .OrderByDescending(menuItem => menuItem.Price)
                .ThenBy(menuItem => menuItem.Name),
            "availability-available-first" => filteredRows
                .OrderByDescending(menuItem => menuItem.IsAvailable)
                .ThenBy(menuItem => menuItem.Name),
            "availability-unavailable-first" => filteredRows
                .OrderBy(menuItem => menuItem.IsAvailable)
                .ThenBy(menuItem => menuItem.Name),
            _ => filteredRows
                .OrderBy(menuItem => GetDiningOrder(menuItem.Category))
                .ThenBy(menuItem => menuItem.Name)
        };

        var viewModel = new AdminMenuItemsIndexViewModel
        {
            Sort = sort,
            Category = category,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Availability = availability,
            PendingAdditionCount = pendingChanges.AddedItems.Count,
            PendingDeletionCount = pendingChanges.DeletedItemIds.Count,
            Items = filteredRows.ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new MenuItem());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItem menuItem)
    {
        if (!ModelState.IsValid)
        {
            return View(menuItem);
        }

        var pendingChanges = GetPendingChanges();
        pendingChanges.AddedItems.Add(new PendingAddedMenuItem
        {
            DraftId = Guid.NewGuid().ToString("N"),
            Name = menuItem.Name,
            Category = menuItem.Category,
            Price = menuItem.Price,
            Description = menuItem.Description,
            Allergens = menuItem.Allergens,
            IsAvailable = menuItem.IsAvailable
        });
        SavePendingChanges(pendingChanges);
        TempData["SuccessMessage"] = "Menu item added to pending changes.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        return menuItem is null ? NotFound() : View(menuItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MenuItem menuItem)
    {
        if (id != menuItem.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Edit", menuItem);
        }

        _context.Update(menuItem);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _context.MenuItems.AnyAsync(menuItem => menuItem.Id == id))
        {
            var pendingChanges = GetPendingChanges();
            if (!pendingChanges.DeletedItemIds.Contains(id))
            {
                pendingChanges.DeletedItemIds.Add(id);
                SavePendingChanges(pendingChanges);
            }
        }

        TempData["SuccessMessage"] = "Delete queued. Click Save Changes to apply it.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UndoDelete(int id)
    {
        var pendingChanges = GetPendingChanges();
        pendingChanges.DeletedItemIds.Remove(id);
        SavePendingChanges(pendingChanges);
        TempData["SuccessMessage"] = "Queued delete removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemovePendingAddition(string draftId)
    {
        var pendingChanges = GetPendingChanges();
        pendingChanges.AddedItems.RemoveAll(menuItem => menuItem.DraftId == draftId);
        SavePendingChanges(pendingChanges);
        TempData["SuccessMessage"] = "Queued item removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveChanges()
    {
        var pendingChanges = GetPendingChanges();
        if (pendingChanges.AddedItems.Count == 0 && pendingChanges.DeletedItemIds.Count == 0)
        {
            TempData["SuccessMessage"] = "There are no pending menu changes to save.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var addedItem in pendingChanges.AddedItems)
        {
            _context.MenuItems.Add(new MenuItem
            {
                Name = addedItem.Name,
                Category = addedItem.Category,
                Price = addedItem.Price,
                Description = addedItem.Description,
                Allergens = addedItem.Allergens,
                IsAvailable = addedItem.IsAvailable
            });
        }

        if (pendingChanges.DeletedItemIds.Count > 0)
        {
            var itemsToDelete = await _context.MenuItems
                .Where(menuItem => pendingChanges.DeletedItemIds.Contains(menuItem.Id))
                .ToListAsync();

            _context.MenuItems.RemoveRange(itemsToDelete);
        }

        await _context.SaveChangesAsync();
        ClearPendingChanges();
        TempData["SuccessMessage"] = "Menu changes saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DiscardChanges()
    {
        ClearPendingChanges();
        TempData["SuccessMessage"] = "Pending menu changes discarded.";
        return RedirectToAction(nameof(Index));
    }

    private PendingMenuChanges GetPendingChanges()
    {
        var json = TempData.Peek(PendingChangesTempDataKey) as string;
        return string.IsNullOrWhiteSpace(json)
            ? new PendingMenuChanges()
            : JsonSerializer.Deserialize<PendingMenuChanges>(json) ?? new PendingMenuChanges();
    }

    private void SavePendingChanges(PendingMenuChanges pendingChanges)
    {
        if (pendingChanges.AddedItems.Count == 0 && pendingChanges.DeletedItemIds.Count == 0)
        {
            TempData.Remove(PendingChangesTempDataKey);
            return;
        }

        TempData[PendingChangesTempDataKey] = JsonSerializer.Serialize(pendingChanges);
    }

    private void ClearPendingChanges()
    {
        TempData.Remove(PendingChangesTempDataKey);
    }

    private static int GetDiningOrder(MenuCategory category) => category switch
    {
        MenuCategory.Beverages => 1,
        MenuCategory.Appetizers => 2,
        MenuCategory.Entrees => 3,
        MenuCategory.Specials => 4,
        _ => 5
    };

    private sealed class PendingMenuChanges
    {
        public List<PendingAddedMenuItem> AddedItems { get; init; } = [];
        public List<int> DeletedItemIds { get; init; } = [];
    }

    private sealed class PendingAddedMenuItem
    {
        public string DraftId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public MenuCategory Category { get; init; }
        public decimal Price { get; init; }
        public string Description { get; init; } = string.Empty;
        public string Allergens { get; init; } = string.Empty;
        public bool IsAvailable { get; init; }
    }
}
