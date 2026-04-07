using FocusGuard.Helpers;
using FocusGuard.Models;
using FocusGuard.Services;

namespace FocusGuard.ViewModels;

/// <summary>
/// ViewModel for the Settings page — binds directly to UserSettings properties.
/// </summary>
public class SettingsViewModel : BaseViewModel
{
    private readonly SessionDataService _dataService;
    private UserSettings _settings;

    public SettingsViewModel(SessionDataService dataService)
    {
        _dataService = dataService;
        _settings = dataService.Settings;
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        ResetDefaultsCommand = new RelayCommand(ResetDefaults);
    }

    // ── Pomodoro Settings ─────────────────────────────────────

    public int WorkMinutes
    {
        get => _settings.WorkMinutes;
        set { _settings.WorkMinutes = Math.Clamp(value, 1, 120); OnPropertyChanged(); }
    }

    public int ShortBreakMinutes
    {
        get => _settings.ShortBreakMinutes;
        set { _settings.ShortBreakMinutes = Math.Clamp(value, 1, 30); OnPropertyChanged(); }
    }

    public int LongBreakMinutes
    {
        get => _settings.LongBreakMinutes;
        set { _settings.LongBreakMinutes = Math.Clamp(value, 1, 60); OnPropertyChanged(); }
    }

    public int SessionsBeforeLongBreak
    {
        get => _settings.SessionsBeforeLongBreak;
        set { _settings.SessionsBeforeLongBreak = Math.Clamp(value, 1, 12); OnPropertyChanged(); }
    }

    public bool AutoStartBreaks
    {
        get => _settings.AutoStartBreaks;
        set { _settings.AutoStartBreaks = value; OnPropertyChanged(); }
    }

    public bool AutoStartWork
    {
        get => _settings.AutoStartWork;
        set { _settings.AutoStartWork = value; OnPropertyChanged(); }
    }

    // ── Eye Care Settings ─────────────────────────────────────

    public bool EyeCareEnabled
    {
        get => _settings.EyeCareEnabled;
        set { _settings.EyeCareEnabled = value; OnPropertyChanged(); }
    }

    public int EyeCareIntervalMinutes
    {
        get => _settings.EyeCareIntervalMinutes;
        set { _settings.EyeCareIntervalMinutes = Math.Clamp(value, 5, 60); OnPropertyChanged(); }
    }

    public int EyeCareDurationSeconds
    {
        get => _settings.EyeCareDurationSeconds;
        set { _settings.EyeCareDurationSeconds = Math.Clamp(value, 10, 120); OnPropertyChanged(); }
    }

    // ── Notification Settings ─────────────────────────────────

    public bool SoundEnabled
    {
        get => _settings.SoundEnabled;
        set { _settings.SoundEnabled = value; OnPropertyChanged(); }
    }

    public bool ToastNotificationsEnabled
    {
        get => _settings.ToastNotificationsEnabled;
        set { _settings.ToastNotificationsEnabled = value; OnPropertyChanged(); }
    }

    // ── General Settings ──────────────────────────────────────

    public bool StartMinimized
    {
        get => _settings.StartMinimized;
        set { _settings.StartMinimized = value; OnPropertyChanged(); }
    }

    public bool MinimizeToTray
    {
        get => _settings.MinimizeToTray;
        set { _settings.MinimizeToTray = value; OnPropertyChanged(); }
    }

    public string Theme
    {
        get => _settings.Theme;
        set { _settings.Theme = value; OnPropertyChanged(); }
    }

    // ── Commands ──────────────────────────────────────────────

    public RelayCommand SaveCommand { get; }
    public RelayCommand ResetDefaultsCommand { get; }

    public event Action? SettingsSaved;

    private async Task SaveAsync()
    {
        await _dataService.SaveSettingsAsync();
        SettingsSaved?.Invoke();
    }

    private void ResetDefaults()
    {
        var defaults = new UserSettings();
        WorkMinutes = defaults.WorkMinutes;
        ShortBreakMinutes = defaults.ShortBreakMinutes;
        LongBreakMinutes = defaults.LongBreakMinutes;
        SessionsBeforeLongBreak = defaults.SessionsBeforeLongBreak;
        AutoStartBreaks = defaults.AutoStartBreaks;
        AutoStartWork = defaults.AutoStartWork;
        EyeCareEnabled = defaults.EyeCareEnabled;
        EyeCareIntervalMinutes = defaults.EyeCareIntervalMinutes;
        EyeCareDurationSeconds = defaults.EyeCareDurationSeconds;
        SoundEnabled = defaults.SoundEnabled;
        ToastNotificationsEnabled = defaults.ToastNotificationsEnabled;
        StartMinimized = defaults.StartMinimized;
        MinimizeToTray = defaults.MinimizeToTray;
        Theme = defaults.Theme;
    }
}
