using FocusGuard.Helpers;
using FocusGuard.Services;

namespace FocusGuard.ViewModels;

/// <summary>
/// Top-level ViewModel that manages navigation between pages
/// and owns child ViewModels.
/// </summary>
public class MainViewModel : BaseViewModel
{
    private readonly SessionDataService _dataService;
    private BaseViewModel _currentPage = null!;
    private string _currentPageName = "Timer";

    public MainViewModel(SessionDataService dataService)
    {
        _dataService = dataService;

        TimerVM = new TimerViewModel(dataService);
        StatsVM = new StatsViewModel(dataService);
        SettingsVM = new SettingsViewModel(dataService);

        NavigateCommand = new RelayCommand(Navigate);
        ShowGuideCommand = new RelayCommand(() => GuideRequested?.Invoke());

        SettingsVM.SettingsSaved += () => TimerVM.ApplySettingsChange();

        _currentPage = TimerVM;
    }

    // ── Child ViewModels ──────────────────────────────────────

    public TimerViewModel TimerVM { get; }
    public StatsViewModel StatsVM { get; }
    public SettingsViewModel SettingsVM { get; }

    // ── Navigation ────────────────────────────────────────────

    public BaseViewModel CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public string CurrentPageName
    {
        get => _currentPageName;
        set => SetProperty(ref _currentPageName, value);
    }

    public RelayCommand NavigateCommand { get; }
    public RelayCommand ShowGuideCommand { get; }

    public event Action? GuideRequested;

    private void Navigate(object? parameter)
    {
        var pageName = parameter as string ?? "Timer";
        CurrentPageName = pageName;

        CurrentPage = pageName switch
        {
            "Timer" => TimerVM,
            "Stats" => StatsVM,
            "Settings" => SettingsVM,
            _ => TimerVM
        };

        if (pageName == "Stats")
            StatsVM.Refresh();
    }

    public void Cleanup()
    {
        TimerVM.Cleanup();
    }
}
