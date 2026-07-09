using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using YumDash.Web.Models;

namespace YumDash.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var definedMigrations = context.Database.GetMigrations();
        if (definedMigrations.Any())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser is null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        if (!await context.MenuItems.AnyAsync())
        {
            var items = new[]
            {
                new MenuItem { Name = "Truffle Fries", Category = MenuCategory.Appetizers, Price = 9.50m, Allergens = "dairy", Description = "Crispy fries with parmesan and herbs.", IsAvailable = true },
                new MenuItem { Name = "Citrus Salmon", Category = MenuCategory.Entrees, Price = 24.00m, Allergens = "fish", Description = "Pan-seared salmon with seasonal vegetables.", IsAvailable = true },
                new MenuItem { Name = "Wild Mushroom Pasta", Category = MenuCategory.Entrees, Price = 19.00m, Allergens = "gluten,dairy", Description = "Creamy mushroom pasta with roasted garlic.", IsAvailable = true },
                new MenuItem { Name = "Chocolate Torte", Category = MenuCategory.Desserts, Price = 8.00m, Allergens = "dairy,eggs", Description = "Rich flourless torte with whipped cream.", IsAvailable = true },
                new MenuItem { Name = "Sparkling Citrus Spritz", Category = MenuCategory.Beverages, Price = 6.50m, Allergens = "", Description = "House sparkling mocktail with orange and mint.", IsAvailable = true },
                new MenuItem { Name = "Chef's Market Special", Category = MenuCategory.Specials, Price = 21.50m, Allergens = "dairy", Description = "Rotating seasonal entree with local vegetables and herb butter.", IsAvailable = true }
            };

            await context.MenuItems.AddRangeAsync(items);
            await context.SaveChangesAsync();
            return;
        }

        var hasSpecial = await context.MenuItems.AnyAsync(menuItem => menuItem.Category == MenuCategory.Specials);
        if (!hasSpecial)
        {
            await context.MenuItems.AddAsync(new MenuItem
            {
                Name = "Chef's Market Special",
                Category = MenuCategory.Specials,
                Price = 21.50m,
                Allergens = "dairy",
                Description = "Rotating seasonal entree with local vegetables and herb butter.",
                IsAvailable = true
            });

            await context.SaveChangesAsync();
        }
    }
}
