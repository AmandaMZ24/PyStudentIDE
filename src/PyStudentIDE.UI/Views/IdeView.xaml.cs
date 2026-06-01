using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PyStudentIDE.UI.Decorators;
using PyStudentIDE.UI.Services;

namespace PyStudentIDE.UI.Views;

public partial class IdeView : UserControl
{
    private string? _currentFile;
    private string? _currentProject;
    private readonly ISignatureStore _signatureStore = new LocalSignatureStore();
    private readonly ScriptSignatureHeaderService _signatureHeader = new();
    private bool _signatureValid = true;

    public IdeView()
    {
        InitializeComponent();
    }

    private void OnNewFile(object sender, RoutedEventArgs e)
    {
        var projectName = PromptForInput("Nuevo Proyecto", "Nombre del proyecto:", "MiProyecto");
        if (string.IsNullOrWhiteSpace(projectName)) return;

        _currentProject = App.ScriptMgr.CreateProject(projectName);
        var scriptName = PromptForInput("Nuevo Script", "Nombre del script:", "main.py");
        if (string.IsNullOrWhiteSpace(scriptName)) return;

        _currentFile = App.ScriptMgr.CreateScript(_currentProject, scriptName);
        CodeEditor.SetText("# Script creado en PyStudentIDE\n");
        CodeEditor.SetReadOnly(false);
        _signatureValid = true;
        ModeIndicator.Text = $"Proyecto: {projectName} - {Path.GetFileName(_currentFile)}";
    }

    private void OnOpenFile(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Python Files|*.py|All Files|*.*" };
        if (dialog.ShowDialog() == true)
        {
            _currentFile = dialog.FileName;
            _currentProject = Path.GetDirectoryName(_currentFile);
            var content = App.ScriptMgr.LoadScript(_currentFile);
            var hasSignature = _signatureHeader.HasSignature(content);
            var isValid = !hasSignature || _signatureHeader.VerifySignature(content, out _);
            _signatureValid = isValid;
            CodeEditor.SetText(_signatureHeader.StripSignature(content));
            CodeEditor.SetReadOnly(!isValid);
            ModeIndicator.Text = isValid
                ? Path.GetFileName(_currentFile)
                : $"Archivo bloqueado (firma inválida): {Path.GetFileName(_currentFile)}";
        }
    }

    private void OnSaveFile(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFile))
        {
            var dialog = new SaveFileDialog { Filter = "Python Files|*.py", DefaultExt = ".py" };
            if (dialog.ShowDialog() == true)
                _currentFile = dialog.FileName;
            else
                return;
        }
        if (!_signatureValid)
        {
            MessageBox.Show("El archivo fue alterado fuera del sistema y está bloqueado.", "Firma inválida", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var signedContent = _signatureHeader.AddOrReplaceSignature(CodeEditor.GetText());
        App.ScriptMgr.SaveScript(_currentFile, signedContent);
        _signatureValid = true;
        MessageBox.Show("Archivo guardado", "PyStudentIDE", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnDeleteFile(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFile))
        {
            MessageBox.Show("No hay un archivo abierto", "PyStudentIDE", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"¿Deseas borrar {Path.GetFileName(_currentFile)}?",
            "Confirmar borrado",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        App.ScriptMgr.DeleteScript(_currentFile);
        _currentFile = null;
        CodeEditor.SetText(string.Empty);
        ModeIndicator.Text = "Archivo eliminado";
    }

    private async void OnRun(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFile))
        {
            MessageBox.Show("Guarde el archivo antes de ejecutar", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!_signatureValid)
        {
            MessageBox.Show("El archivo tiene una firma inválida. Corrija o regenere la firma antes de ejecutar.", "Firma inválida", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        App.ScriptMgr.SaveScript(_currentFile, CodeEditor.GetText());

        Terminal.AppendOutput($"> Ejecutando {Path.GetFileName(_currentFile)}...\n");

        try
        {
            var psi = new ProcessStartInfo("python", _currentFile)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) { Terminal.AppendOutput("Error: No se pudo iniciar Python\n"); return; }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            Terminal.AppendOutput(output);
            if (!string.IsNullOrEmpty(error))
                Terminal.AppendOutput($"ERROR:\n{error}\n");
            Terminal.AppendOutput($"> Proceso finalizado (código: {process.ExitCode})\n");
        }
        catch (Exception ex)
        {
            Terminal.AppendOutput($"Error al ejecutar: {ex.Message}\n");
        }
    }

    private void OnSignScript(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFile))
        {
            MessageBox.Show("Guarde el archivo antes de firmarlo", "PyStudentIDE", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var signedContent = _signatureHeader.AddOrReplaceSignature(CodeEditor.GetText());
        App.ScriptMgr.SaveScript(_currentFile, signedContent);
        var hash = _signatureHeader.ComputeHash(CodeEditor.GetText());
        _signatureValid = true;
        CodeEditor.SetReadOnly(false);
        MessageBox.Show($"Firma generada: {hash[..Math.Min(16, hash.Length)]}...", "Firma", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnVerifyScript(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFile))
        {
            MessageBox.Show("Guarde el archivo antes de verificar", "PyStudentIDE", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var content = App.ScriptMgr.LoadScript(_currentFile);
        var hasSignature = _signatureHeader.HasSignature(content);
        var isValid = hasSignature && _signatureHeader.VerifySignature(content, out _);
        _signatureValid = isValid;
        CodeEditor.SetReadOnly(!isValid);
        var message = isValid ? "Firma válida ✅" : "Firma inválida ❌";
        MessageBox.Show(message, "Verificación", MessageBoxButton.OK, isValid ? MessageBoxImage.Information : MessageBoxImage.Error);
    }

    private void OnRunTests(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Función de pruebas automáticas disponible en el servidor", "Pruebas", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private string? PromptForInput(string title, string prompt, string defaultValue)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            ResizeMode = ResizeMode.NoResize
        };
        var stack = new StackPanel { Margin = new Thickness(10) };
        stack.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10) });
        var textBox = new TextBox { Text = defaultValue, Margin = new Thickness(0, 0, 0, 10) };
        stack.Children.Add(textBox);
        var button = new Button { Content = "Aceptar", Height = 30, Width = 100, HorizontalAlignment = HorizontalAlignment.Center };
        stack.Children.Add(button);
        dialog.Content = stack;
        string? result = null;
        button.Click += (_, _) => { result = textBox.Text; dialog.Close(); };
        textBox.KeyDown += (_, args) => { if (args.Key == Key.Enter) { result = textBox.Text; dialog.Close(); } };
        dialog.ShowDialog();
        return result;
    }
}
