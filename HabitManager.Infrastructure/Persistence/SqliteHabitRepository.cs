using HabitManager.Application;
using HabitManager.Domain.Entities;
using HabitManager.Domain.Enums;
using HabitManager.Domain.ValueObjects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HabitManager.Infrastructure.Persistence;

public sealed class SqliteHabitRepository(HabitDbContext database) : IHabitRepository
{
    public async Task<IReadOnlyList<Habit>> ListAsync(string userId)
    {
        var records = await database.Habits.AsNoTracking()
            .Where(habit => habit.UserId == userId)
            .Include(habit => habit.Completions)
            .OrderBy(habit => habit.CreatedAt).ThenBy(habit => habit.Id)
            .ToListAsync();

        return records.Select(ToDomain).ToArray();
    }

    public async Task<Habit?> FindAsync(string userId, Guid id)
    {
        var record = await database.Habits
            .Include(habit => habit.Completions)
            .SingleOrDefaultAsync(habit => habit.Id == id && habit.UserId == userId);

        return record is null ? null : ToDomain(record);
    }

    public async Task SaveAsync(Habit habit)
    {
        // Keep the tracked version loaded by FindAsync so an overlapping write
        // cannot silently overwrite a newer aggregate.
        var record = database.Habits.Local.SingleOrDefault(item => item.Id == habit.Id)
            ?? await database.Habits.Include(item => item.Completions)
                .SingleOrDefaultAsync(item => item.Id == habit.Id);
        if (record is null)
        {
            record = new HabitRecord { Id = habit.Id, UserId = habit.UserId, CreatedAt = habit.CreatedAt };
            database.Habits.Add(record);
        }

        record.Name = habit.Name;
        record.Description = habit.Description;
        record.Status = habit.Status;
        record.RepeatType = habit.Schedule.RepeatType;
        record.WeekdaysMask = habit.Schedule.DaysOfWeek.Aggregate(0, (mask, day) => mask | (1 << (int)day));
        record.OnceDate = habit.Schedule.OnceDate;
        record.UpdatedAt = habit.UpdatedAt;
        record.Version = Guid.NewGuid();

        var dates = habit.Completions.Select(item => item.Date).ToHashSet();
        var removed = record.Completions.Where(item => !dates.Contains(item.Date)).ToArray();
        database.Completions.RemoveRange(removed);
        foreach (var completion in removed)
            record.Completions.Remove(completion);

        var existingIds = record.Completions.Select(item => item.Id).ToHashSet();
        foreach (var completion in habit.Completions.Where(item => !existingIds.Contains(item.Id)))
        {
            record.Completions.Add(new CompletionRecord
            {
                Id = completion.Id,
                HabitId = habit.Id,
                Date = completion.Date
            });
        }

        try
        {
            // EF saves the aggregate and its completion changes in one transaction.
            await database.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException error)
        {
            throw new HabitStorageConflictException(
                "The habit changed during this operation. Refresh and try again.", error);
        }
        catch (DbUpdateException error) when (error.InnerException is SqliteException
            { SqliteExtendedErrorCode: 2067 or 1555 })
        {
            throw new HabitStorageConflictException(
                "This record already exists. Refresh the habit and try again.", error);
        }
    }

    private static Habit ToDomain(HabitRecord record)
    {
        var schedule = record.RepeatType switch
        {
            RepeatType.Daily => HabitSchedule.Daily(),
            RepeatType.Once when record.OnceDate is DateOnly date => HabitSchedule.Once(date),
            RepeatType.Weekly => HabitSchedule.Weekly(Enum.GetValues<DayOfWeek>()
                .Where(day => (record.WeekdaysMask & (1 << (int)day)) != 0).ToArray()),
            _ => throw new InvalidOperationException("The database contains an invalid schedule.")
        };

        return Habit.Rehydrate(
            record.Id, record.UserId, record.Name, record.Description, schedule,
            record.Status, record.CreatedAt, record.UpdatedAt,
            record.Completions.Select(item => new HabitCompletion(item.Id, item.HabitId, item.Date)));
    }
}

