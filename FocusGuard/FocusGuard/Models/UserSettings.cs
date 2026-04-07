namespace FocusGuard.Models;

/// <summary>
/// Stores all user-configurable settings, persisted as JSON.
/// </summary>
public class UserSettings
{
    // Pomodoro Timer
    public int WorkMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int SessionsBeforeLongBreak { get; set; } = 4;
    public bool AutoStartBreaks { get; set; } = true;
    public bool AutoStartWork { get; set; } = false;

    // Eye Care (20-20-20 Rule)
    public bool EyeCareEnabled { get; set; } = true;
    public int EyeCareIntervalMinutes { get; set; } = 20;
    public int EyeCareDurationSeconds { get; set; } = 20;

    // Notifications
    public bool SoundEnabled { get; set; } = true;
    public bool ToastNotificationsEnabled { get; set; } = true;
    public double Volume { get; set; } = 0.7;

    // General
    public bool StartMinimized { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool LaunchAtStartup { get; set; } = false;
    public bool ShowGuideOnStartup { get; set; } = true;
    public string Theme { get; set; } = "Dark";

    // Window state
    public double WindowLeft { get; set; } = -1;
    public double WindowTop { get; set; } = -1;
    public double WindowWidth { get; set; } = 900;
    public double WindowHeight { get; set; } = 620;
}
