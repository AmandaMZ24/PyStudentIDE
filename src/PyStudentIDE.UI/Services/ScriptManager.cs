using System.IO;

namespace PyStudentIDE.UI.Services;

public class ScriptManager
{
    private readonly string _basePath;

    public ScriptManager()
    {
        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PyStudentIDE", "Projects");
        Directory.CreateDirectory(_basePath);
    }

    public string CreateProject(string projectName)
    {
        var path = Path.Combine(_basePath, projectName);
        Directory.CreateDirectory(path);
        return path;
    }

    public string CreateScript(string projectPath, string scriptName)
    {
        var filePath = Path.Combine(projectPath, scriptName.EndsWith(".py") ? scriptName : scriptName + ".py");
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "# Script creado en PyStudentIDE\n");
        return filePath;
    }

    public void SaveScript(string filePath, string content) =>
        File.WriteAllText(filePath, content);

    public string LoadScript(string filePath) =>
        File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

    public void DeleteScript(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    public string[] GetProjects() =>
        Directory.GetDirectories(_basePath)
            .Select(Path.GetFileName)
            .Where(x => x != null)
            .Cast<string>()
            .ToArray();

    public string[] GetScripts(string projectName)
    {
        var path = Path.Combine(_basePath, projectName);
        return Directory.Exists(path)
            ? Directory.GetFiles(path, "*.py").Select(Path.GetFileName).Cast<string>().ToArray()
            : Array.Empty<string>();
    }

    public string GetProjectPath(string projectName) =>
        Path.Combine(_basePath, projectName);
}
