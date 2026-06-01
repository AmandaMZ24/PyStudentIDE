using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace PyStudentIDE.UI.Controls;

public partial class TerminalControl : UserControl
{
    private Process? _pythonProcess;

    public TerminalControl()
    {
        InitializeComponent();
        StartPythonRepl();
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
        if (_pythonProcess != null && !_pythonProcess.HasExited)
        {
            _pythonProcess.StandardInput.WriteLine(command);
            _pythonProcess.StandardInput.Flush();
        }
    }
}
