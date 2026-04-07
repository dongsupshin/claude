using System.ComponentModel;
using System.Windows;
using SnipVault.ViewModels;
using SnipVault.Views;

namespace SnipVault;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        vm.GuideRequested += ShowGuide;
    }

    public void ShowGuide()
    {
        var guide = new GuideWindow { Owner = this };
        guide.ShowDialog();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
    }
}
