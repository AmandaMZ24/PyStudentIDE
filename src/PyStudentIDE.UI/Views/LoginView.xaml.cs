using System.Windows;
using System.Windows.Controls;
using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.UI.Views;

public partial class LoginView : UserControl
{
    private readonly MainWindow _mainWindow;

    public LoginView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = EmailBox.Text.Trim(),
                Password = PasswordBox.Password
            };

            var result = await App.Api.LoginAsync(request);
            if (result != null)
            {
                App.CurrentUserId = result.UsuarioId;
                App.CurrentUserName = result.Nombre;
                App.CurrentUserRole = result.Rol;
                App.CurrentToken = result.Token;
                _mainWindow.ShowDashboard();
            }
            else
            {
                ErrorText.Text = "Credenciales inválidas";
                ErrorText.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            ErrorText.Text = $"Error de conexión: {ex.Message}";
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
