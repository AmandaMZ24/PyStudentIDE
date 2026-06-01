using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace PyStudentIDE.UI.Controls;

public partial class PythonEditor : UserControl
{
    public PythonEditor()
    {
        InitializeComponent();
        CommandManager.AddPreviewExecutedHandler(EditorTextBox, OnPreviewExecutedCommand);
    }

    public string GetText() => EditorTextBox.Text;
    public void SetText(string text) => EditorTextBox.Text = text;
    public void SetReadOnly(bool isReadOnly) => EditorTextBox.IsReadOnly = isReadOnly;

    private void OnPreviewExecutedCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Paste)
        {
            e.Handled = true;
            ShowPasteWarning();
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if ((e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) ||
            (e.Key == Key.Insert && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift))
        {
            e.Handled = true;
            ShowPasteWarning();
        }
    }

    private void ShowPasteWarning()
    {
        MessageBox.Show(
            "El pegado de código no está permitido en este IDE.",
            "Acción bloqueada",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
