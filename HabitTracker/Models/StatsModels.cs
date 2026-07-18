namespace HabitTracker.Models;

public class WeekBlock
{
    public int WeekNumber { get; set; }
    public string ColorHex { get; set; } = "#f4a825";
    public List<DateOnly> Days { get; set; } = new();
    public int Completed { get; set; }
    public int Incomplete { get; set; }
    public int Total => Completed + Incomplete;
    public double Percentage => Total == 0 ? 0 : Math.Round(Completed * 100.0 / Total, 2);
}

public class HabitRow
{
    public Habit Habit { get; set; } = null!;
    public Dictionary<DateOnly, bool> DayStatus { get; set; } = new();
    public int CompletedCount { get; set; }
    public double GoalProgressPercent { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
}

public class MonthlyTrackerData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<DateOnly> Days { get; set; } = new();
    public List<HabitRow> HabitRows { get; set; } = new();
    public List<WeekBlock> Weeks { get; set; } = new();
    public int TotalCompleted { get; set; }
    public int TotalIncomplete { get; set; }
    public double TotalPercentage => (TotalCompleted + TotalIncomplete) == 0
        ? 0
        : Math.Round(TotalCompleted * 100.0 / (TotalCompleted + TotalIncomplete), 2);

    // per-day totals for the "Habits Completed / Incomplete" footer rows
    public Dictionary<DateOnly, int> CompletedPerDay { get; set; } = new();
    public Dictionary<DateOnly, int> IncompletePerDay { get; set; } = new();
}

public class DashboardStats
{
    public string BestHabitName { get; set; } = "-";
    public double BestHabitPercent { get; set; }
    public string NeedsWorkHabitName { get; set; } = "-";
    public double NeedsWorkHabitPercent { get; set; }
    public int LongestStreakOverall { get; set; }
    public string LongestStreakHabitName { get; set; } = "-";
    public List<(string Month, double Percent)> Last6MonthsTrend { get; set; } = new();
    public List<(string HabitName, double Percent, string Color)> HabitBreakdown { get; set; } = new();
}
