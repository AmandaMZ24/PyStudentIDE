using System.Diagnostics;
using PyStudentIDE.Application.Interfaces;

namespace PyStudentIDE.Infrastructure.Git;

public class GitAdapter : IGitAdapter
{
    public string Init(string repoPath)
    {
        return RunGitCommand(repoPath, "init");
    }

    public string Clone(string url, string localPath)
    {
        return RunGitCommand(localPath, $"clone \"{url}\" \"{localPath}\"");
    }

    public string Add(string repoPath, string filePattern)
    {
        return RunGitCommand(repoPath, $"add {filePattern}");
    }

    public string Commit(string repoPath, string message)
    {
        return RunGitCommand(repoPath, $"commit -m \"{message.Replace("\"", "\\\"")}\"");
    }

    public string Push(string repoPath)
    {
        return RunGitCommand(repoPath, "push");
    }

    public string Pull(string repoPath)
    {
        return RunGitCommand(repoPath, "pull");
    }

    public string Status(string repoPath)
    {
        return RunGitCommand(repoPath, "status");
    }

    public string Log(string repoPath)
    {
        return RunGitCommand(repoPath, "log --oneline -20");
    }

    private static string RunGitCommand(string repoPath, string arguments)
    {
        var psi = new ProcessStartInfo("git", arguments)
        {
            WorkingDirectory = repoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Could not start git process");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException($"Git error: {error}");
        return output.Trim();
    }
}
