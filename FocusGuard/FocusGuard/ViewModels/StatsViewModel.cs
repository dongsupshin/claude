using System.Collections.ObjectModel;
using FocusGuard.Helpers;
using FocusGuard.Models;
using FocusGuard.Services;

namespace FocusGuard.ViewModels;

/// <summary>
/// Data object for a single bar in the weekly chart (pure WPF, no external chart library).
/// </summary>
public class ChartBar : BaseViewModel
{
    public string Label { get; set; } = "";
    public double Value { get; set; }
    public double MaxValue { get; set; } = 1;
    public double HeightRatio => MaxValue > 0 ? Value / MaxValue : 0;
    public string DisplayValue => Value > 0 ? $"{Value:0}" : "";
}

/// <summary>
/// ViewModel for the Statistics dashboard — uses pure WPF data for rendering charts.
/// No external chart library required.
/// </summary>
public class StatsViewModel : BaseViewModel
{
    private readonly SessionDataService _dataService;

    private int _totalSessions;
    private int _completedSessions;
    private int _totalMinutes;
    private int _currentStreak;
    private string _completionRate = "0%";

    public StatsViewModel(SessionDataService dataService)
    {
        _dataService = dataService;
        RefreshCommand = new RelayCommand(Refresh);
        FocusBars = new ObservableCollection<ChartBar>();
        SessionBars = new ObservableCollection<ChartBar>();
    }

    // ── Properties ────────────────────────────────────────────

    public int TotalSessions { get => _totalSessions; set => SetProperty(ref _totalSessions, value); }
    public int CompletedSessions { get => _completedSessions; set => SetProperty(ref _completedSessions, value); }
    public int TotalMinutes { get => _totalMinutes; set => SetProperty(ref _totalMinutes, value); }
    public int CurrentStreak { get => _currentStreak; set => SetProperty(ref _currentStreak, value); }
    public string CompletionRate { get => _completionRate; set => SetProperty(ref _completionRate, value); }

    public ObservableCollection<ChartBar> FocusBars { get; }
    public ObservableCollection<ChartBar> SessionBars { get; }

    public RelayCommand RefreshCommand { get; }

    // ── Methods ───────────────────────────────────────────────

    public void Refresh()
    {
        var (total, completed, minutes, streak) = _dataService.GetOverallStats();
        TotalSessions = total;
        CompletedSessions = completed;
        TotalMinutes = minutes;
        CurrentStreak = streak;
        CompletionRate = total > 0 ? $"{(completed * 100 / total)}%" : "N/A";

        UpdateChart();
    }

    private void UpdateChart()
    {
        var weekly = _dataService.GetWeeklySummary();

        double maxFocus = Math.Max(1, weekly.Max(d => d.TotalFocusMinutes));
        double maxSessions = Math.Max(1, weekly.Max(d => d.CompletedSessions));

        FocusBars.Clear();
        SessionBars.Clear();

        foreach (var day in weekly)
        {
            FocusBars.Add(new ChartBar
            {
                Label = day.Date.ToString("ddd"),
                Value = day.TotalFocusMinutes,
                MaxValue = maxFocus
            });

            SessionBars.Add(new ChartBar
            {
                Label = day.Date.ToString("ddd"),
                Value = day.CompletedSessions,
                MaxValue = maxSessions
            });
        }
    }
}
