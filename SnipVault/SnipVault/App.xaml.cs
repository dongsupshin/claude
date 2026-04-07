using System.Windows;
using System.Windows.Threading;
using SnipVault.Services;
using SnipVault.ViewModels;
using SnipVault.Views;

namespace SnipVault;

public partial class App : Application
{
    private DataService _dataService = null!;
    private MainViewModel _mainVm = null!;
    private MainWindow _mainWindow = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnUnhandled;
        AppDomain.CurrentDomain.UnhandledException += OnFatal;
        TaskScheduler.UnobservedTaskException += OnTask;

        try
        {
            _dataService = new DataService();
            await _dataService.LoadSettingsAsync();
            await _dataService.LoadSnippetsAsync();

            _mainVm = new MainViewModel(_dataService);
            _mainWindow = new MainWindow(_mainVm);
            _mainWindow.Show();

            _mainVm.Initialize();

            if (_dataService.Settings.ShowGuideOnStartup)
            {
                _mainWindow.ShowGuide();
                _dataService.Settings.ShowGuideOnStartup = false;
                await _dataService.SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup Error:\n\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                "SnipVault - Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnUnhandled(object s, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Error:\n\n{e.Exception.Message}\n\nInner: {e.Exception.InnerException?.Message}",
            "SnipVault - Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnFatal(object s, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            MessageBox.Show($"Fatal:\n{ex.Message}", "SnipVault", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnTask(object? s, UnobservedTaskExceptionEventArgs e)
    {
        MessageBox.Show($"Task Error:\n{e.Exception?.Message}", "SnipVault", MessageBoxButton.OK, MessageBoxImage.Error);
        e.SetObserved();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try { if (_dataService != null) await _dataService.SaveSettingsAsync(); } catch { }
        base.OnExit(e);
    }
}
