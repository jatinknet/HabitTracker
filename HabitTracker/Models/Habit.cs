using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models;

public class Habit
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Habit name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Hex color used for accents (checkbox ticks, progress bars) chosen by the user per habit
    [MaxLength(20)]
    public string ColorHex { get; set; } = "#f4a825";

    // Bootstrap Icons class, e.g. "bi-cup-hot", "bi-book", "bi-droplet"
    [MaxLength(50)]
    public string Icon { get; set; } = "bi-check2-circle";

    [Range(1, 366)]
    public int MonthlyGoal { get; set; } = 30;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HabitEntry> Entries { get; set; } = new List<HabitEntry>();
}
