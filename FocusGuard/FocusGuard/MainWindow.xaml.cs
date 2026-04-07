using System.ComponentModel;
using System.Windows;
using FocusGuard.ViewModels;
using FocusGuard.Views;

namespace FocusGuard;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.GuideRequested += ShowGuide;
    }

    public void ShowGuide()
    {
        var guide = new GuideWindow { Owner = this };
        guide.ShowDialog();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _viewModel.Cleanup();
        base.OnClosing(e);
    }
}
