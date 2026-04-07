using System.Windows;
using System.Windows.Threading;
using FocusGuard.Services;
using FocusGuard.ViewModels;
using FocusGuard.Views;

namespace FocusGuard;

public partial class App : Application
{
    private SessionDataService _dataService = null!;
    private MainViewModel _mainViewModel = null!;
    private MainWindow _mainWindow = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handlers for debugging
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            // Initialize data service
            _dataService = new SessionDataService();
            await _dataService.LoadSettingsAsync();
            await _dataService.LoadSessionsAsync();

            // Create main ViewModel
            _mainViewModel = new MainViewModel(_dataService);

            // Create and show main window
            _mainWindow = new MainWindow(_mainViewModel);

            if (_dataService.Settings.StartMinimized)
            {
                _mainWindow.WindowState = WindowState.Minimized;
                _mainWindow.Show();
            }
            else
            {
                _mainWindow.Show();
            }

            // Show guide on first launch
            if (_dataService.Settings.ShowGuideOnStartup)
            {
                _mainWindow.ShowGuide();
                _dataService.Settings.ShowGuideOnStartup = false;
                await _dataService.SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Startup Error:\n\n{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "FocusGuard - Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Unhandled Error:\n\n{e.Exception.GetType().Name}: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}\n\nInner: {e.Exception.InnerException?.Message}",
            "FocusGuard - Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show(
                $"Fatal Error:\n\n{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "FocusGuard - Fatal Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Task Error:\n\n{e.Exception?.GetType().Name}: {e.Exception?.Message}",
            "FocusGuard - Task Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.SetObserved();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_dataService != null)
        {
            try { await _dataService.SaveSettingsAsync(); } catch { }
        }
        base.OnExit(e);
    }
}
