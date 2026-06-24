namespace PyStudentIDE.Application.Services;

public interface IGitService
{
    string Init(int cursoId, string repoPath);
    string Clone(int cursoId, string url, string localPath);
    string Add(int cursoId, string repoPath, string filePattern);
    string Commit(int cursoId, string repoPath, string message, int entregaId = 0);
    string Push(int cursoId, string repoPath);
    string Pull(int cursoId, string repoPath);
    string Status(int cursoId, string repoPath);
    string Log(int cursoId, string repoPath);
}
