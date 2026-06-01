using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;
using DecoratorApp.Decorators;

namespace DecoratorApp.Views;

public partial class ScriptDecoratorView : UserControl
{
    private readonly ISignatureStore _signatureStore = new LocalSignatureStore();
    private string? _currentPath;
    private SignedScriptDecorator? _signed;
    private FormattedScriptDecorator? _formatted;

    public ScriptDecoratorView()
    {
        InitializeComponent();
        StatusText.Text = "Carga un script para visualizarlo y firmarlo.";
    }

    private void OnLoadScript(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Python Files|*.py|All Files|*.*" };
        if (dialog.ShowDialog() == true)
        {
            _currentPath = dialog.FileName;
            LoadScriptFromFile();
        }
    }

    private void OnReload(object sender, RoutedEventArgs e)
    {
        LoadScriptFromFile();
    }

    private void LoadScriptFromFile()
    {
        if (string.IsNullOrWhiteSpace(_currentPath) || !File.Exists(_currentPath))
        {
            StatusText.Text = "Selecciona un archivo válido.";
            return;
        }

        var text = File.ReadAllText(_currentPath);
        var baseScript = new Script(_currentPath, text);
        _signed = new SignedScriptDecorator(baseScript, _signatureStore);
        _formatted = new FormattedScriptDecorator(_signed);

        FilePathText.Text = _currentPath;
        RenderHighlightedText();
        StatusText.Text = "Script cargado.";
    }

    private void RenderHighlightedText()
    {
        if (_formatted == null) return;
        ScriptViewer.Document.Blocks.Clear();
        var paragraph = new Paragraph { Margin = new Thickness(8) };

        foreach (var segment in _formatted.GetHighlightedSegments())
        {
            var run = new Run(segment.Text) { Foreground = segment.Foreground };
            paragraph.Inlines.Add(run);
        }

        ScriptViewer.Document.Blocks.Add(paragraph);
    }

    private void OnSign(object sender, RoutedEventArgs e)
    {
        if (_signed == null)
        {
            StatusText.Text = "Primero carga un script.";
            return;
        }

        var hash = _signed.GenerateSignature();
        StatusText.Text = $"Firma generada: {hash[..Math.Min(16, hash.Length)]}...";
    }

    private void OnVerify(object sender, RoutedEventArgs e)
    {
        if (_signed == null)
        {
            StatusText.Text = "Primero carga un script.";
            return;
        }

        var isValid = _signed.VerifySignature();
        StatusText.Text = isValid ? "Firma válida ✅" : "Firma inválida ❌";
    }

    private void OnRegenerate(object sender, RoutedEventArgs e)
    {
        if (_signed == null)
        {
            StatusText.Text = "Primero carga un script.";
            return;
        }

        var hash = _signed.RegenerateSignature();
        StatusText.Text = $"Firma regenerada: {hash[..Math.Min(16, hash.Length)]}...";
    }
}
