namespace PyStudentIDE.UI.Decorators;

public interface ISignatureStore
{
    ScriptSignature? GetByPath(string scriptPath);
    void Save(ScriptSignature signature);
}
