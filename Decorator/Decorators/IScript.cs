namespace DecoratorApp.Decorators;

public interface IScript
{
    string FilePath { get; }
    string Text { get; }
    string GetPath();
    string GetText();
}
