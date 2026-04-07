using System.IO;
using System.Text.Json;
using FocusGuard.Models;

namespace FocusGuard.Services;

/// <summary>
/// Handles persistence of focus sessions and user settings using local JSON files.
/// Data is stored in %APPDATA%/FocusGuard/.
/// </summary>
public class SessionDataService
{
    private static readonly string AppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusGuard");

    private static readonly string SessionsFile = Path.Combine(AppDataFolder, "sessions.json");
    private static readonly string SettingsFile = Path.Combine(AppDataFolder, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private List<FocusSession> _sessions = new();
    private UserSettings _settings = new();

    public IReadOnlyList<FocusSession> Sessions => _sessions.AsReadOnly();
    public UserSettings Settings => _settings;

    public SessionDataService()
    {
        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(AppDataFolder))
            Directory.CreateDirectory(AppDataFolder);
    }

    // ── Settings ──────────────────────────────────────────────

    public async Task<UserSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = await File.ReadAllTextAsync(SettingsFile);
                _settings = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions) ?? new UserSettings();
            }
        }
        catch
        {
            _settings = new UserSettings();
        }
        return _settings;
    }

    public async Task SaveSettingsAsync()
    {
        var json = JsonSerializer.Serialize(_settings, JsonOptions);
        await File.WriteAllTextAsync(SettingsFile, json);
    }

    // ── Focus Sessions ────────────────────────────────────────

    public async Task LoadSessionsAsync()
    {
        try
        {
            if (File.Exists(SessionsFile))
            {
                var json = await File.ReadAllTextAsync(SessionsFile);
                _sessions = JsonSerializer.Deserialize<List<FocusSession>>(json, JsonOptions) ?? new();
            }
        }
        catch
        {
            _sessions = new();
        }
    }

    public async Task SaveSessionAsync(FocusSession session)
    {
        _sessions.Add(session);
        // Keep only last 90 days of data
        var cutoff = DateTime.Now.AddDays(-90);
        _sessions.RemoveAll(s => s.StartTime < cutoff);

        var json = JsonSerializer.Serialize(_sessions, JsonOptions);
        await File.WriteAllTextAsync(SessionsFile, json);
    }

    // ── Statistics Queries ────────────────────────────────────

    public List<FocusSession> GetSessionsForDate(DateTime date)
    {
        return _sessions.Where(s => s.StartTime.Date == date.Date).ToList();
    }

    public List<DailySummary> GetWeeklySummary()
    {
        var today = DateTime.Today;
        var summaries = new List<DailySummary>();

        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var daySessions = GetSessionsForDate(date);
            summaries.Add(new DailySummary
            {
                Date = date,
                TotalSessions = daySessions.Count,
                CompletedSessions = daySessions.Count(s => s.Completed),
                TotalFocusMinutes = (int)daySessions.Sum(s => s.ActualMinutes)
            });
        }

        return summaries;
    }

    public (int totalSessions, int completedSessions, int totalMinutes, int streak) GetOverallStats()
    {
        int totalSessions = _sessions.Count;
        int completedSessions = _sessions.Count(s => s.Completed);
        int totalMinutes = (int)_sessions.Sum(s => s.ActualMinutes);

        // Calculate streak (consecutive days with at least one completed session)
        int streak = 0;
        var date = DateTime.Today;
        while (true)
        {
            var daySessions = GetSessionsForDate(date);
            if (daySessions.Any(s => s.Completed))
            {
                streak++;
                date = date.AddDays(-1);
            }
            else break;
        }

        return (totalSessions, completedSessions, totalMinutes, streak);
    }
}
