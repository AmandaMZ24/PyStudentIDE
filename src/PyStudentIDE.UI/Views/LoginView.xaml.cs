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

    private void OnShowRegister(object sender, RoutedEventArgs e)
    {
        LoginPanel.Visibility = Visibility.Collapsed;
        RegisterPanel.Visibility = Visibility.Visible;
        ErrorText.Visibility = Visibility.Collapsed;
    }

    private void OnShowLogin(object sender, RoutedEventArgs e)
    {
        RegisterPanel.Visibility = Visibility.Collapsed;
        LoginPanel.Visibility = Visibility.Visible;
        RegErrorText.Visibility = Visibility.Collapsed;
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
                _mainWindow.SetupRoleUI();
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

    private async void OnRegisterClick(object sender, RoutedEventArgs e)
    {
        RegErrorText.Visibility = Visibility.Collapsed;

        var nombre = RegNameBox.Text.Trim();
        var email = RegEmailBox.Text.Trim();
        var password = RegPasswordBox.Password;
        var confirm = RegConfirmBox.Password;

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            RegErrorText.Text = "Todos los campos son obligatorios";
            RegErrorText.Visibility = Visibility.Visible;
            return;
        }

        if (password != confirm)
        {
            RegErrorText.Text = "Las contraseñas no coinciden";
            RegErrorText.Visibility = Visibility.Visible;
            return;
        }

        if (password.Length < 6)
        {
            RegErrorText.Text = "La contraseña debe tener al menos 6 caracteres";
            RegErrorText.Visibility = Visibility.Visible;
            return;
        }

        var rolTag = ((ComboBoxItem)RegRolBox.SelectedItem)?.Tag?.ToString();
        if (!int.TryParse(rolTag, out var idRol))
        {
            RegErrorText.Text = "Seleccione un rol válido";
            RegErrorText.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var request = new RegisterRequest
            {
                Nombre = nombre,
                Email = email,
                Password = password,
                IdRol = idRol
            };

            var result = await App.Api.RegisterAsync(request);
            if (result != null)
            {
                App.CurrentUserId = result.UsuarioId;
                App.CurrentUserName = result.Nombre;
                App.CurrentUserRole = result.Rol;
                App.CurrentToken = result.Token;
                _mainWindow.SetupRoleUI();
                _mainWindow.ShowDashboard();
            }
            else
            {
                RegErrorText.Text = "El correo ya está registrado";
                RegErrorText.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            RegErrorText.Text = $"Error de conexión: {ex.Message}";
            RegErrorText.Visibility = Visibility.Visible;
        }
    }
}
