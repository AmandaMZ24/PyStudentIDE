using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace PyStudentIDE.UI.Controls;

public partial class TerminalControl : UserControl
{
    private Process? _pythonProcess;
    private string? _currentRepoPath;

    public TerminalControl()
    {
        InitializeComponent();
        StartPythonRepl();
    }

    public void SetRepoPath(string repoPath)
    {
        _currentRepoPath = repoPath;
    }

    private void StartPythonRepl()
    {
        try
        {
            _pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo("python")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _pythonProcess.Start();

            _ = Task.Run(async () =>
            {
                while (!_pythonProcess.StandardOutput.EndOfStream)
                {
                    var line = await _pythonProcess.StandardOutput.ReadLineAsync();
                    if (line != null)
                        Dispatcher.Invoke(() => AppendOutput(line + "\n"));
                }
            });
            _ = Task.Run(async () =>
            {
                while (!_pythonProcess.StandardError.EndOfStream)
                {
                    var line = await _pythonProcess.StandardError.ReadLineAsync();
                    if (line != null)
                        Dispatcher.Invoke(() => AppendOutput($"ERROR: {line}\n"));
                }
            });
        }
        catch (Exception ex)
        {
            AppendOutput($"Error al iniciar Python: {ex.Message}\n");
        }
    }

    public void AppendOutput(string text)
    {
        OutputBox.AppendText(text);
        OutputBox.ScrollToEnd();
    }

    private void OnInputKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var command = InputBox.Text;
            InputBox.Clear();
            ExecuteCommand(command);
        }
    }

    private void ExecuteCommand(string command)
    {
        AppendOutput($">>> {command}\n");

        if (command.StartsWith("git ", StringComparison.OrdinalIgnoreCase))
        {
            ExecuteGitCommand(command);
            return;
        }

        if (_pythonProcess != null && !_pythonProcess.HasExited)
        {
            _pythonProcess.StandardInput.WriteLine(command);
            _pythonProcess.StandardInput.Flush();
        }
        else
        {
            AppendOutput("Python REPL no está disponible. Use 'git' para comandos de control de versiones.\n");
        }
    }

    private void ExecuteGitCommand(string command)
    {
        var args = command.Length > 4 ? command[4..] : "";
        var repoPath = _currentRepoPath ?? Environment.CurrentDirectory;

        try
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                AppendOutput("Error: No se pudo iniciar Git\n");
                return;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(30000);

            if (!string.IsNullOrEmpty(output))
                AppendOutput(output);
            if (!string.IsNullOrEmpty(error))
                AppendOutput($"ERROR: {error}\n");
            AppendOutput($"> Git finalizado (código: {process.ExitCode})\n");
        }
        catch (Exception ex)
        {
            AppendOutput($"Error al ejecutar Git: {ex.Message}\n");
        }
    }

    public void Stop()
    {
        try
        {
            _pythonProcess?.Kill();
            _pythonProcess?.Dispose();
        }
        catch { }
    }
}
