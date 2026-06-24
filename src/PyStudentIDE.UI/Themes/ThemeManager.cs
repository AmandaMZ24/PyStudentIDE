using System.Windows;

namespace PyStudentIDE.UI.Themes;

public static class ThemeManager
{
    private const string DarkThemePath = "Themes/DarkTheme.xaml";
    private const string LightThemePath = "Themes/LightTheme.xaml";

    public static bool IsDarkTheme { get; private set; } = true;

    public static event Action? ThemeChanged;

    public static void LoadTheme(bool isDark)
    {
        IsDarkTheme = isDark;
        var uri = new Uri(isDark ? DarkThemePath : LightThemePath, UriKind.RelativeOrAbsolute);
        var dict = new ResourceDictionary { Source = uri };

        var app = System.Windows.Application.Current;
        var existing = app.Resources.MergedDictionaries
            .FirstOrDefault(d =>
                d.Source?.ToString().Contains("DarkTheme") == true ||
                d.Source?.ToString().Contains("LightTheme") == true);

        if (existing != null)
            app.Resources.MergedDictionaries.Remove(existing);

        app.Resources.MergedDictionaries.Add(dict);
        ThemeChanged?.Invoke();
    }

    public static void Toggle()
    {
        LoadTheme(!IsDarkTheme);
    }
}
