using BookingService.Api.Core.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<BlockedTime> BlockedTimes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
     entity.HasKey(e => e.Id);
       entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
     entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
          entity.HasIndex(e => e.Email).IsUnique();
          entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    });

      // Resource configuration
        modelBuilder.Entity<Resource>(entity =>
        {
       entity.HasKey(e => e.Id);
       entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

      // Reservation configuration
        modelBuilder.Entity<Reservation>(entity =>
  {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Status).HasConversion<int>();
          entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        entity.HasOne(e => e.User)
           .WithMany(u => u.Reservations)
         .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Restrict);

       entity.HasOne(e => e.Resource)
                .WithMany(r => r.Reservations)
   .HasForeignKey(e => e.ResourceId)
.OnDelete(DeleteBehavior.Cascade);

     entity.HasIndex(e => new { e.ResourceId, e.StartTime, e.EndTime });
        });

        // BlockedTime configuration
        modelBuilder.Entity<BlockedTime>(entity =>
        {
entity.HasKey(e => e.Id);
         entity.Property(e => e.Reason).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

 entity.HasOne(e => e.Resource)
     .WithMany(r => r.BlockedTimes)
    .HasForeignKey(e => e.ResourceId)
         .OnDelete(DeleteBehavior.Cascade);

 entity.HasIndex(e => new { e.ResourceId, e.StartTime, e.EndTime });
   });
    }
}
