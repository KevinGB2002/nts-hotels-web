using Microsoft.EntityFrameworkCore;
using NtsHotels.Api.Models;

namespace NtsHotels.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Name).IsRequired().HasMaxLength(150);
            e.Property(h => h.Address).IsRequired().HasMaxLength(200);
            e.Property(h => h.City).IsRequired().HasMaxLength(100);
            e.HasIndex(h => h.Name);
            e.HasIndex(h => h.Rating);
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.Property(r => r.Name).IsRequired().HasMaxLength(50);
            e.HasIndex(r => r.Name).IsUnique();
            e.HasData(new Role { Id = 1, Name = "Admin" }, new Role { Id = 2, Name = "User" });
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.FullName).IsRequired().HasMaxLength(150);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Role).WithMany().HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
