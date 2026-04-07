using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SnipVault.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
    {
        bool v = value is bool b && b;
        if (parameter is string s && s == "Invert") v = !v;
        return v ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => value is Visibility vis && vis == Visibility.Visible;
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
        => value is bool b ? !b : value;
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => value is bool b ? !b : value;
}

public class FavoriteToColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
        => value is bool b && b
            ? new SolidColorBrush(Color.FromRgb(251, 191, 36))   // Gold
            : new SolidColorBrush(Color.FromRgb(75, 85, 99));    // Gray
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => throw new NotImplementedException();
}

public class PinToColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
        => value is bool b && b
            ? new SolidColorBrush(Color.FromRgb(96, 165, 250))   // Blue
            : new SolidColorBrush(Color.FromRgb(75, 85, 99));    // Gray
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => throw new NotImplementedException();
}

public class StringNotEmptyToVisibility : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
        => value is string s && !string.IsNullOrWhiteSpace(s)
            ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => throw new NotImplementedException();
}

public class DateTimeToRelativeConverter : IValueConverter
{
    public object Convert(object value, Type t, object parameter, CultureInfo c)
    {
        if (value is not DateTime dt) return "";
        var diff = DateTime.Now - dt;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
        return dt.ToString("MMM d, yyyy");
    }
    public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
        => throw new NotImplementedException();
}
