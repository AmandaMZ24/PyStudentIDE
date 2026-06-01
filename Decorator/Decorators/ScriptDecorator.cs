namespace DecoratorApp.Decorators;

public abstract class ScriptDecorator : IScript
{
    protected ScriptDecorator(IScript inner) => Inner = inner;

    protected IScript Inner { get; }

    public virtual string FilePath => Inner.FilePath;
    public virtual string Text => Inner.Text;

    public virtual string GetPath() => Inner.GetPath();
    public virtual string GetText() => Inner.GetText();
}
