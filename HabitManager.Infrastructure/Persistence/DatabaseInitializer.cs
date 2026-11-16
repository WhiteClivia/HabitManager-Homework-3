using HabitManager.Domain.TestData;
using Microsoft.EntityFrameworkCore;

namespace HabitManager.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(HabitDbContext database, bool seedDemoData)
    {
        var created = await database.Database.EnsureCreatedAsync();
        if (!created || !seedDemoData)
            return;

        await using var transaction = await database.Database.BeginTransactionAsync();
        var repository = new SqliteHabitRepository(database);
        foreach (var habit in TestDataGenerator.GenerateHabits())
            await repository.SaveAsync(habit);

        await transaction.CommitAsync();
    }
}
