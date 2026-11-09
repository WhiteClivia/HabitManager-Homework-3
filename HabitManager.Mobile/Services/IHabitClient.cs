using HabitManager.Contracts;

namespace HabitManager.Mobile.Services;

public interface IHabitClient
{
    Task<HabitDto[]> ListAsync();
    Task<HabitDto> GetAsync(Guid id);
    Task CreateAsync(CreateHabitRequest request);
    Task UpdateAsync(Guid id, UpdateHabitRequest request);
    Task SetArchivedAsync(Guid id, bool archived);
    Task SetCompletedAsync(Guid id, DateOnly date, bool completed);
}
