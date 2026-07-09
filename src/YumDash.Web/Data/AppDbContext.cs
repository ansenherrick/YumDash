using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YumDash.Web.Models;

namespace YumDash.Web.Data;

public class AppDbContext : IdentityDbContext<AppUser>, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<EventInquiry> EventInquiries => Set<EventInquiry>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MenuItem>()
            .Property(menuItem => menuItem.Price)
            .HasPrecision(10, 2);

        builder.Entity<Reservation>()
            .Property(reservation => reservation.EstimatedSpendPerGuest)
            .HasPrecision(10, 2);
    }
}
