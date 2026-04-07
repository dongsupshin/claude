namespace FocusGuard.Models;

/// <summary>
/// Represents a single focus (Pomodoro) session record.
/// </summary>
public class FocusSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int PlannedMinutes { get; set; }
    public int ActualSeconds { get; set; }
    public bool Completed { get; set; }
    public string Tag { get; set; } = "General";

    public double ActualMinutes => ActualSeconds / 60.0;
}

/// <summary>
/// Represents a daily summary of focus activity.
/// </summary>
public class DailySummary
{
    public DateTime Date { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public int TotalFocusMinutes { get; set; }
    public int EyeCareBreaksTaken { get; set; }
}
