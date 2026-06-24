using System.Windows;
using System.Windows.Controls;
using PyStudentIDE.UI.Themes;
using PyStudentIDE.UI.Views;

namespace PyStudentIDE.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoginContent.Content = new LoginView(this);
        UpdateThemeButton();
        Themes.ThemeManager.ThemeChanged += UpdateThemeButton;
    }

    private void UpdateThemeButton()
    {
        ThemeToggle.Content = Themes.ThemeManager.IsDarkTheme ? "☀️ Modo Claro" : "🌙 Modo Oscuro";
    }

    public void ShowLogin()
    {
        App.CurrentUserId = 0;
        App.CurrentToken = string.Empty;
        MainLayout.Visibility = Visibility.Collapsed;
        LoginContent.Visibility = Visibility.Visible;
    }

    public void ShowDashboard()
    {
        LoginContent.Visibility = Visibility.Collapsed;
        MainLayout.Visibility = Visibility.Visible;
        MainContent.Content = new DashboardView();
    }

    public void ShowIde()
    {
        LoginContent.Visibility = Visibility.Collapsed;
        MainLayout.Visibility = Visibility.Visible;
        MainContent.Content = new IdeView();
    }

    public void ShowAssignments()
    {
        LoginContent.Visibility = Visibility.Collapsed;
        MainLayout.Visibility = Visibility.Visible;
        MainContent.Content = new AssignmentPanelView();
    }

    public void ShowTeacherPanel()
    {
        LoginContent.Visibility = Visibility.Collapsed;
        MainLayout.Visibility = Visibility.Visible;
        MainContent.Content = new TeacherPanelView();
    }

    public void ShowDecorator()
    {
        LoginContent.Visibility = Visibility.Collapsed;
        MainLayout.Visibility = Visibility.Visible;
        MainContent.Content = new ScriptDecoratorView();
    }

    public void SetupRoleUI()
    {
        var isTeacher = App.CurrentUserRole == "DOCENTE";
        TeacherNavBtn.Visibility = isTeacher ? Visibility.Visible : Visibility.Collapsed;
        StudentNavBtn.Visibility = isTeacher ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnDashboardClick(object sender, RoutedEventArgs e) => ShowDashboard();
    private void OnEditorClick(object sender, RoutedEventArgs e) => ShowIde();
    private void OnAssignmentsClick(object sender, RoutedEventArgs e) => ShowAssignments();
    private void OnTeacherPanelClick(object sender, RoutedEventArgs e) => ShowTeacherPanel();
    private void OnTerminalClick(object sender, RoutedEventArgs e) => ShowIde();
    private void OnDecoratorClick(object sender, RoutedEventArgs e) => ShowDecorator();
    private void OnLogoutClick(object sender, RoutedEventArgs e) => ShowLogin();
    private void OnToggleTheme(object sender, RoutedEventArgs e) => Themes.ThemeManager.Toggle();
}
