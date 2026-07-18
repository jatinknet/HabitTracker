using HabitTracker.Data;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

public class HabitService : IHabitService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    // Week color palette, mirrors the original spreadsheet's week banding
    private static readonly string[] WeekColors =
    {
        "#f4a825", // Week 1 - orange
        "#2e86de", // Week 2 - blue
        "#e74c3c", // Week 3 - red
        "#27ae60", // Week 4 - green
        "#8e44ad", // Week 5 - purple
        "#16a085"  // Week 6 (rare, months starting on Saturday) - teal
    };

    public HabitService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Habit>> GetActiveHabitsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Habits
            .Where(h => h.IsActive)
            .OrderBy(h => h.SortOrder)
            .ThenBy(h => h.Id)
            .ToListAsync();
    }

    public async Task<Habit> AddHabitAsync(Habit habit)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var maxSort = await db.Habits.AnyAsync() ? await db.Habits.MaxAsync(h => h.SortOrder) : 0;
        habit.SortOrder = maxSort + 1;
        habit.CreatedAt = DateTime.UtcNow;
        db.Habits.Add(habit);
        await db.SaveChangesAsync();
        return habit;
    }

    public async Task UpdateHabitAsync(Habit habit)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Habits.Update(habit);
        await db.SaveChangesAsync();
    }

    public async Task DeleteHabitAsync(int habitId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var habit = await db.Habits.FindAsync(habitId);
        if (habit is null) return;

        // Soft delete keeps history intact; flip IsActive instead of removing rows
        habit.IsActive = false;
        await db.SaveChangesAsync();
    }

    public async Task<bool> ToggleEntryAsync(int habitId, DateOnly date)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var entry = await db.HabitEntries
            .FirstOrDefaultAsync(e => e.HabitId == habitId && e.Date == date);

        if (entry is null)
        {
            entry = new HabitEntry { HabitId = habitId, Date = date, Completed = true };
            db.HabitEntries.Add(entry);
        }
        else
        {
            entry.Completed = !entry.Completed;
        }

        await db.SaveChangesAsync();
        return entry.Completed;
    }

    public async Task<MonthlyTrackerData> GetMonthlyDataAsync(int year, int month)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var habits = await db.Habits
            .Where(h => h.IsActive)
            .OrderBy(h => h.SortOrder)
            .ThenBy(h => h.Id)
            .ToListAsync();

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var days = Enumerable.Range(1, daysInMonth)
            .Select(d => new DateOnly(year, month, d))
            .ToList();

        var firstDay = days.First();
        var lastDay = days.Last();

        var entries = await db.HabitEntries
            .Where(e => e.Date >= firstDay && e.Date <= lastDay)
            .ToListAsync();

        var data = new MonthlyTrackerData { Year = year, Month = month, Days = days };

        // Build week blocks of 7 days each, starting at day 1 (matches the reference tracker)
        var weekBlocks = new List<WeekBlock>();
        for (int i = 0; i < days.Count; i += 7)
        {
            var weekDays = days.Skip(i).Take(7).ToList();
            weekBlocks.Add(new WeekBlock
            {
                WeekNumber = weekBlocks.Count + 1,
                ColorHex = WeekColors[weekBlocks.Count % WeekColors.Length],
                Days = weekDays
            });
        }

        foreach (var habit in habits)
        {
            var habitEntries = entries.Where(e => e.HabitId == habit.Id).ToDictionary(e => e.Date, e => e.Completed);
            var row = new HabitRow { Habit = habit };

            foreach (var day in days)
            {
                var completed = habitEntries.TryGetValue(day, out var c) && c;
                row.DayStatus[day] = completed;
                if (completed) row.CompletedCount++;
            }

            row.GoalProgressPercent = habit.MonthlyGoal == 0
                ? 0
                : Math.Min(100, Math.Round(row.CompletedCount * 100.0 / habit.MonthlyGoal, 1));

            (row.CurrentStreak, row.LongestStreak) = ComputeStreaks(row.DayStatus, days);

            data.HabitRows.Add(row);

            // Fold this habit's per-day results into the week blocks & per-day totals
            foreach (var week in weekBlocks)
            {
                foreach (var day in week.Days)
                {
                    var completed = row.DayStatus[day];
                    if (completed) week.Completed++; else week.Incomplete++;
                }
            }

            foreach (var day in days)
            {
                var completed = row.DayStatus[day];
                data.CompletedPerDay[day] = data.CompletedPerDay.GetValueOrDefault(day) + (completed ? 1 : 0);
                data.IncompletePerDay[day] = data.IncompletePerDay.GetValueOrDefault(day) + (completed ? 0 : 1);
            }
        }

        data.Weeks = weekBlocks;
        data.TotalCompleted = weekBlocks.Sum(w => w.Completed);
        data.TotalIncomplete = weekBlocks.Sum(w => w.Incomplete);

        return data;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(int year, int month)
    {
        var monthly = await GetMonthlyDataAsync(year, month);
        var stats = new DashboardStats();

        if (monthly.HabitRows.Any())
        {
            var byGoalPercent = monthly.HabitRows
                .Select(r => (r.Habit.Name, Percent: r.Habit.MonthlyGoal == 0 ? 0 : r.GoalProgressPercent, r.Habit.ColorHex))
                .OrderByDescending(x => x.Percent)
                .ToList();

            var best = byGoalPercent.First();
            var worst = byGoalPercent.Last();

            stats.BestHabitName = best.Name;
            stats.BestHabitPercent = best.Percent;
            stats.NeedsWorkHabitName = worst.Name;
            stats.NeedsWorkHabitPercent = worst.Percent;

            stats.HabitBreakdown = byGoalPercent.Select(x => (x.Name, x.Percent, x.ColorHex)).ToList();

            var bestStreakRow = monthly.HabitRows.OrderByDescending(r => r.LongestStreak).First();
            stats.LongestStreakOverall = bestStreakRow.LongestStreak;
            stats.LongestStreakHabitName = bestStreakRow.Habit.Name;
        }

        // Last 6 months completion trend (based on total completion % across all habits)
        var trend = new List<(string Month, double Percent)>();
        for (int i = 5; i >= 0; i--)
        {
            var refDate = new DateTime(year, month, 1).AddMonths(-i);
            var mData = i == 0 ? monthly : await GetMonthlyDataAsync(refDate.Year, refDate.Month);
            trend.Add((refDate.ToString("MMM"), mData.TotalPercentage));
        }
        stats.Last6MonthsTrend = trend;

        return stats;
    }

    private static (int current, int longest) ComputeStreaks(Dictionary<DateOnly, bool> dayStatus, List<DateOnly> orderedDays)
    {
        int longest = 0, running = 0, current = 0;
        var today = DateOnly.FromDateTime(DateTime.Now);

        foreach (var day in orderedDays)
        {
            if (dayStatus.TryGetValue(day, out var done) && done)
            {
                running++;
                longest = Math.Max(longest, running);
            }
            else
            {
                running = 0;
            }
        }

        // "current" streak = consecutive completed days ending today (or the last tracked day)
        for (int i = orderedDays.Count - 1; i >= 0; i--)
        {
            var day = orderedDays[i];
            if (day > today) continue;
            if (dayStatus.TryGetValue(day, out var done) && done)
                current++;
            else
                break;
        }

        return (current, longest);
    }
}
