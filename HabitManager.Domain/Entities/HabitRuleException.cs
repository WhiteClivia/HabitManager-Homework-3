namespace HabitManager.Domain.Entities;

public sealed class HabitRuleException(string message) : InvalidOperationException(message);

