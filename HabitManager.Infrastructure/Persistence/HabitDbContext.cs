using HabitManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HabitManager.Infrastructure.Persistence;

public sealed class HabitDbContext(DbContextOptions<HabitDbContext> options) : DbContext(options)
{
    public DbSet<HabitRecord> Habits => Set<HabitRecord>();
    public DbSet<CompletionRecord> Completions => Set<CompletionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var habits = modelBuilder.Entity<HabitRecord>();
        habits.ToTable("Habits", table =>
        {
            table.HasCheckConstraint("CK_Habits_Name", "length(trim(Name)) BETWEEN 1 AND 100");
            table.HasCheckConstraint("CK_Habits_Description", "length(Description) <= 1000");
            table.HasCheckConstraint("CK_Habits_UserId", "length(trim(UserId)) > 0");
            table.HasCheckConstraint("CK_Habits_Status",
                $"Status IN ({(int)HabitStatus.Active}, {(int)HabitStatus.Archived})");
            table.HasCheckConstraint("CK_Habits_Schedule",
                $"(RepeatType = {(int)RepeatType.Daily} AND WeekdaysMask = 0 AND OnceDate IS NULL) OR " +
                $"(RepeatType = {(int)RepeatType.Once} AND WeekdaysMask = 0 AND OnceDate IS NOT NULL) OR " +
                $"(RepeatType = {(int)RepeatType.Weekly} AND WeekdaysMask BETWEEN 1 AND 127 AND OnceDate IS NULL)");
        });
        habits.HasKey(habit => habit.Id);
        habits.Property(habit => habit.Id).ValueGeneratedNever();
        habits.Property(habit => habit.Name).HasMaxLength(100).IsRequired();
        habits.Property(habit => habit.Description).HasMaxLength(1000).IsRequired();
        habits.Property(habit => habit.UserId).IsRequired();
        habits.Property(habit => habit.CreatedAt).HasConversion(
            value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        habits.Property(habit => habit.UpdatedAt).HasConversion(
            value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        habits.Property(habit => habit.Version).IsConcurrencyToken();
        habits.HasIndex(habit => habit.UserId);
        habits.HasMany(habit => habit.Completions).WithOne()
            .HasForeignKey(completion => completion.HabitId).OnDelete(DeleteBehavior.Cascade);

        var completions = modelBuilder.Entity<CompletionRecord>();
        completions.ToTable("HabitCompletions");
        completions.HasKey(completion => completion.Id);
        completions.Property(completion => completion.Id).ValueGeneratedNever();
        completions.HasIndex(completion => new { completion.HabitId, completion.Date }).IsUnique();
    }
}

