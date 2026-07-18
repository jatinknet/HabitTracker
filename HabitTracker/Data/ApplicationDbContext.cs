using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitEntry> HabitEntries => Set<HabitEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasIndex(h => h.SortOrder);
        });

        modelBuilder.Entity<HabitEntry>(entity =>
        {
            entity.HasIndex(e => new { e.HabitId, e.Date }).IsUnique();

            entity.HasOne(e => e.Habit)
                  .WithMany(h => h.Entries)
                  .HasForeignKey(e => e.HabitId)
                  .OnDelete(DeleteBehavior.Cascade);

            // DateOnly <-> SQL Server "date" column
            entity.Property(e => e.Date).HasColumnType("date");
        });

        // Seed a couple of starter habits so the app isn't empty on first run
        modelBuilder.Entity<Habit>().HasData(
            new Habit { Id = 1, Name = "Drink 16 fl oz of Water", ColorHex = "#f4a825", Icon = "bi-droplet-fill", MonthlyGoal = 30, SortOrder = 1, CreatedAt = new DateTime(2026, 1, 1) },
            new Habit { Id = 2, Name = "Play Tennis", ColorHex = "#2e86de", Icon = "bi-trophy-fill", MonthlyGoal = 10, SortOrder = 2, CreatedAt = new DateTime(2026, 1, 1) }
        );
    }
}
