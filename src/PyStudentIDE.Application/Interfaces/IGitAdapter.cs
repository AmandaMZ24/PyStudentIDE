namespace PyStudentIDE.Application.Interfaces;

public interface IGitAdapter
{
    string Init(string repoPath);
    string Clone(string url, string localPath);
    string Add(string repoPath, string filePattern);
    string Commit(string repoPath, string message);
    string Push(string repoPath);
    string Pull(string repoPath);
    string Status(string repoPath);
    string Log(string repoPath);
}
