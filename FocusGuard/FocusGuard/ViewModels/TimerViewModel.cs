using System.Windows.Threading;
using FocusGuard.Helpers;
using FocusGuard.Models;
using FocusGuard.Services;

namespace FocusGuard.ViewModels;

/// <summary>
/// ViewModel for the Pomodoro Timer and Eye Care features.
/// </summary>
public class TimerViewModel : BaseViewModel
{
    private readonly SessionDataService _dataService;
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _eyeCareTimer;

    private int _remainingSeconds;
    private bool _isRunning;
    private bool _isBreak;
    private int _completedSessions;
    private int _currentSessionNumber;
    private string _statusText = "Ready to focus";
    private string _phaseText = "WORK";
    private double _progress;
    private int _totalPhaseSeconds;
    private DateTime _sessionStartTime;

    // Eye Care
    private bool _isEyeCareActive;
    private int _eyeCareRemainingSeconds;
    private string _eyeCareStatusText = "";

    public TimerViewModel(SessionDataService dataService)
    {
        _dataService = dataService;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;

        _eyeCareTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _eyeCareTimer.Tick += OnEyeCareTick;

        StartCommand = new RelayCommand(Start, () => !IsRunning);
        PauseCommand = new RelayCommand(Pause, () => IsRunning);
        ResetCommand = new RelayCommand(Reset);
        SkipCommand = new RelayCommand(Skip);
        DismissEyeCareCommand = new RelayCommand(DismissEyeCare);

        _currentSessionNumber = 1;
        InitializeTimer();
    }

    // ── Properties ────────────────────────────────────────────

    public int RemainingSeconds
    {
        get => _remainingSeconds;
        set
        {
            if (SetProperty(ref _remainingSeconds, value))
            {
                OnPropertyChanged(nameof(TimeDisplay));
                UpdateProgress();
            }
        }
    }

    public string TimeDisplay
    {
        get
        {
            int mins = _remainingSeconds / 60;
            int secs = _remainingSeconds % 60;
            return $"{mins:D2}:{secs:D2}";
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set => SetProperty(ref _isRunning, value);
    }

    public bool IsBreak
    {
        get => _isBreak;
        set => SetProperty(ref _isBreak, value);
    }

    public int CompletedSessions
    {
        get => _completedSessions;
        set => SetProperty(ref _completedSessions, value);
    }

    public int CurrentSessionNumber
    {
        get => _currentSessionNumber;
        set => SetProperty(ref _currentSessionNumber, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string PhaseText
    {
        get => _phaseText;
        set => SetProperty(ref _phaseText, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public bool IsEyeCareActive
    {
        get => _isEyeCareActive;
        set => SetProperty(ref _isEyeCareActive, value);
    }

    public int EyeCareRemainingSeconds
    {
        get => _eyeCareRemainingSeconds;
        set
        {
            if (SetProperty(ref _eyeCareRemainingSeconds, value))
                OnPropertyChanged(nameof(EyeCareTimeDisplay));
        }
    }

    public string EyeCareTimeDisplay => $"{_eyeCareRemainingSeconds}s";

    public string EyeCareStatusText
    {
        get => _eyeCareStatusText;
        set => SetProperty(ref _eyeCareStatusText, value);
    }

    // ── Commands ──────────────────────────────────────────────

    public RelayCommand StartCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand SkipCommand { get; }
    public RelayCommand DismissEyeCareCommand { get; }

    // ── Events ────────────────────────────────────────────────

    public event Action? PhaseCompleted;
    public event Action? EyeCareReminderTriggered;

    // ── Methods ───────────────────────────────────────────────

    private void InitializeTimer()
    {
        var settings = _dataService.Settings;
        _totalPhaseSeconds = settings.WorkMinutes * 60;
        RemainingSeconds = _totalPhaseSeconds;
        IsBreak = false;
        PhaseText = "WORK";
        StatusText = $"Focus Session {CurrentSessionNumber} — Ready";
        Progress = 0;
    }

    private void Start()
    {
        if (!IsRunning)
        {
            IsRunning = true;
            _sessionStartTime = DateTime.Now;
            StatusText = IsBreak
                ? "Take a break — relax your mind"
                : $"Focus Session {CurrentSessionNumber} — In Progress";
            _timer.Start();

            // Start eye care timer if enabled and in work mode
            if (_dataService.Settings.EyeCareEnabled && !IsBreak)
                StartEyeCareTimer();
        }
    }

    private void Pause()
    {
        if (IsRunning)
        {
            IsRunning = false;
            StatusText = IsBreak ? "Break Paused" : "Session Paused";
            _timer.Stop();
            _eyeCareTimer.Stop();
        }
    }

    private void Reset()
    {
        _timer.Stop();
        _eyeCareTimer.Stop();
        IsRunning = false;
        IsEyeCareActive = false;
        InitializeTimer();
    }

    private void Skip()
    {
        _timer.Stop();
        _eyeCareTimer.Stop();
        IsRunning = false;
        IsEyeCareActive = false;
        OnPhaseComplete();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        RemainingSeconds--;

        if (RemainingSeconds <= 0)
        {
            _timer.Stop();
            _eyeCareTimer.Stop();
            IsRunning = false;
            OnPhaseComplete();
        }
    }

    private async void OnPhaseComplete()
    {
        var settings = _dataService.Settings;

        if (!IsBreak)
        {
            // Work session completed
            CompletedSessions++;
            if (settings.SoundEnabled)
                SoundService.PlayWorkComplete();

            // Save session record
            var session = new FocusSession
            {
                StartTime = _sessionStartTime,
                EndTime = DateTime.Now,
                PlannedMinutes = settings.WorkMinutes,
                ActualSeconds = _totalPhaseSeconds - _remainingSeconds,
                Completed = _remainingSeconds <= 0
            };
            await _dataService.SaveSessionAsync(session);

            // Switch to break
            IsBreak = true;
            bool isLongBreak = CompletedSessions % settings.SessionsBeforeLongBreak == 0;
            _totalPhaseSeconds = (isLongBreak ? settings.LongBreakMinutes : settings.ShortBreakMinutes) * 60;
            RemainingSeconds = _totalPhaseSeconds;
            PhaseText = isLongBreak ? "LONG BREAK" : "SHORT BREAK";
            StatusText = isLongBreak
                ? "Great work! Take a long break"
                : "Nice! Take a short break";
            Progress = 0;

            if (settings.AutoStartBreaks)
                Start();
        }
        else
        {
            // Break completed
            if (settings.SoundEnabled)
                SoundService.PlayBreakComplete();

            IsBreak = false;
            CurrentSessionNumber = CompletedSessions + 1;
            _totalPhaseSeconds = settings.WorkMinutes * 60;
            RemainingSeconds = _totalPhaseSeconds;
            PhaseText = "WORK";
            StatusText = $"Focus Session {CurrentSessionNumber} — Ready";
            Progress = 0;

            if (settings.AutoStartWork)
                Start();
        }

        PhaseCompleted?.Invoke();
    }

    private void UpdateProgress()
    {
        if (_totalPhaseSeconds > 0)
            Progress = 1.0 - ((double)_remainingSeconds / _totalPhaseSeconds);
    }

    // ── Eye Care ──────────────────────────────────────────────

    private void StartEyeCareTimer()
    {
        _eyeCareTimer.Stop();
        _eyeCareRemainingSeconds = _dataService.Settings.EyeCareIntervalMinutes * 60;
        _eyeCareTimer.Start();
    }

    private void OnEyeCareTick(object? sender, EventArgs e)
    {
        if (IsEyeCareActive)
        {
            EyeCareRemainingSeconds--;
            if (EyeCareRemainingSeconds <= 0)
            {
                DismissEyeCare();
            }
        }
        else
        {
            _eyeCareRemainingSeconds--;
            if (_eyeCareRemainingSeconds <= 0)
            {
                // Trigger eye care reminder
                IsEyeCareActive = true;
                EyeCareRemainingSeconds = _dataService.Settings.EyeCareDurationSeconds;
                EyeCareStatusText = "Look at something 20 feet away for 20 seconds";
                if (_dataService.Settings.SoundEnabled)
                    SoundService.PlayEyeCareReminder();
                EyeCareReminderTriggered?.Invoke();
            }
        }
    }

    private void DismissEyeCare()
    {
        IsEyeCareActive = false;
        EyeCareStatusText = "";
        // Restart eye care countdown
        _eyeCareRemainingSeconds = _dataService.Settings.EyeCareIntervalMinutes * 60;
    }

    public void ApplySettingsChange()
    {
        if (!IsRunning)
            InitializeTimer();
    }

    public void Cleanup()
    {
        _timer.Stop();
        _eyeCareTimer.Stop();
    }
}
