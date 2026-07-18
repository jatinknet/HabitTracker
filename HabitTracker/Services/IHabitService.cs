using HabitTracker.Models;

namespace HabitTracker.Services;

public interface IHabitService
{
    Task<List<Habit>> GetActiveHabitsAsync();
    Task<Habit> AddHabitAsync(Habit habit);
    Task UpdateHabitAsync(Habit habit);
    Task DeleteHabitAsync(int habitId);
    Task<bool> ToggleEntryAsync(int habitId, DateOnly date);
    Task<MonthlyTrackerData> GetMonthlyDataAsync(int year, int month);
    Task<DashboardStats> GetDashboardStatsAsync(int year, int month);
}
