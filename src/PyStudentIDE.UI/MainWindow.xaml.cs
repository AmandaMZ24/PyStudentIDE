using System.Windows;
using System.Windows.Controls;
using PyStudentIDE.UI.Views;

namespace PyStudentIDE.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ShowLogin();
    }

    public void ShowLogin()
    {
        MainContent.Content = new LoginView(this);
    }

    public void ShowDashboard()
    {
        MainContent.Content = new DashboardView();
    }

    public void ShowIde()
    {
        MainContent.Content = new IdeView();
    }

    public void ShowAssignments()
    {
        MainContent.Content = new AssignmentPanelView();
    }

    public void ShowDecorator()
    {
        MainContent.Content = new ScriptDecoratorView();
    }

    private void OnDashboardClick(object sender, RoutedEventArgs e) => ShowDashboard();
    private void OnEditorClick(object sender, RoutedEventArgs e) => ShowIde();
    private void OnAssignmentsClick(object sender, RoutedEventArgs e) => ShowAssignments();
    private void OnTerminalClick(object sender, RoutedEventArgs e) => ShowIde();
    private void OnDecoratorClick(object sender, RoutedEventArgs e) => ShowDecorator();
    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        App.CurrentUserId = 0;
        App.CurrentToken = string.Empty;
        ShowLogin();
    }
}
