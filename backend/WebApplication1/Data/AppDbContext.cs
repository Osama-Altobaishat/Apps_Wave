using BookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasOne(r => r.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(r => r.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Resource)
                  .WithMany(r => r.Bookings)
                  .HasForeignKey(b => b.ResourceId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Helpful index for overlap checks per resource
            entity.HasIndex(b => new { b.ResourceId, b.StartAt, b.EndAt });
        });
    }
}
