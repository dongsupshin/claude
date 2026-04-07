using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnipVault.Views;

public partial class GuideWindow : Window
{
    private int _currentPage;
    private readonly StackPanel[] _pages;
    private readonly Ellipse[] _dots;

    public GuideWindow()
    {
        InitializeComponent();
        _pages = new[] { Page1, Page2, Page3 };
        _dots = new[] { Dot1, Dot2, Dot3 };
    }

    private void UpdatePage()
    {
        for (int i = 0; i < _pages.Length; i++)
        {
            _pages[i].Visibility = i == _currentPage ? Visibility.Visible : Visibility.Collapsed;
            _dots[i].Fill = i == _currentPage
                ? new SolidColorBrush(Color.FromRgb(99, 102, 241))
                : new SolidColorBrush(Color.FromRgb(55, 65, 81));
        }

        BackButton.Visibility = _currentPage > 0 ? Visibility.Visible : Visibility.Collapsed;

        if (NextButton.Template.FindName("NextText", NextButton) is TextBlock tb)
            tb.Text = _currentPage == _pages.Length - 1 ? "Get Started! 🚀" : "Next →";
    }

    private void OnNext(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _pages.Length - 1)
        {
            _currentPage++;
            UpdatePage();
        }
        else
        {
            DialogResult = true;
            Close();
        }
    }

    private void OnBack(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            UpdatePage();
        }
    }
}
