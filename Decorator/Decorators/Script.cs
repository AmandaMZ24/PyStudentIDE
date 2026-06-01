namespace DecoratorApp.Decorators;

public class Script : IScript
{
    public Script(string filePath, string text)
    {
        FilePath = filePath;
        Text = text;
    }

    public string FilePath { get; }
    public string Text { get; }

    public string GetPath() => FilePath;
    public string GetText() => Text;
}
