using System.Text.RegularExpressions;
using System.Text;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Domain.Entities;

namespace PyStudentIDE.Application.Services;

public class GitService : IGitService
{
    private readonly IGitAdapter _gitAdapter;
    private readonly ILlaveCursoService _llaveService;
    private readonly IUnitOfWork _unitOfWork;

    public GitService(IGitAdapter gitAdapter, ILlaveCursoService llaveService, IUnitOfWork unitOfWork)
    {
        _gitAdapter = gitAdapter;
        _llaveService = llaveService;
        _unitOfWork = unitOfWork;
    }

    public string Init(int cursoId, string repoPath)
    {
        return _gitAdapter.Init(repoPath);
    }

    public string Clone(int cursoId, string url, string localPath)
    {
        return _gitAdapter.Clone(url, localPath);
    }

    public string Add(int cursoId, string repoPath, string filePattern)
    {
        return _gitAdapter.Add(repoPath, filePattern);
    }

    public string Commit(int cursoId, string repoPath, string message, int entregaId = 0)
    {
        var pyFiles = Directory.GetFiles(repoPath, "*.py", SearchOption.AllDirectories);
        foreach (var file in pyFiles)
        {
            var contenido = File.ReadAllBytes(file);
            var contentStr = File.ReadAllText(file);

            if (!contentStr.StartsWith("# SIGNATURE:", StringComparison.OrdinalIgnoreCase))
                continue;

            var firmaLine = contentStr.Split('\n')[0].Trim();
            var firma = firmaLine.Replace("# SIGNATURE:", "").Trim();
            var bodyBytes = Encoding.UTF8.GetBytes(
                string.Join("\n", contentStr.Split('\n').Skip(1)).TrimEnd());

            if (!_llaveService.VerificarFirma(bodyBytes, firma, cursoId))
                throw new InvalidOperationException(
                    $"El archivo {Path.GetFileName(file)} no pasó la validación criptográfica. Commit rechazado.");
        }

        _gitAdapter.Add(repoPath, ".");
        var output = _gitAdapter.Commit(repoPath, message);

        if (entregaId > 0)
        {
            var hash = ExtractCommitHash(output);
            if (!string.IsNullOrEmpty(hash))
            {
                var commitGit = new CommitGit
                {
                    IdEntrega = entregaId,
                    HashCommit = hash,
                    Mensaje = message,
                    Rama = "main",
                    FechaCommit = DateTime.UtcNow
                };
                _unitOfWork.Repository<CommitGit>().Add(commitGit);
                _unitOfWork.SaveChanges();
            }
        }

        return output;
    }

    private static string ExtractCommitHash(string gitOutput)
    {
        var match = Regex.Match(gitOutput, @"\[.*?([a-f0-9]{7,40})\]");
        return match.Success ? match.Groups[1].Value : "";
    }

    public string Push(int cursoId, string repoPath)
    {
        return _gitAdapter.Push(repoPath);
    }

    public string Pull(int cursoId, string repoPath)
    {
        return _gitAdapter.Pull(repoPath);
    }

    public string Status(int cursoId, string repoPath)
    {
        return _gitAdapter.Status(repoPath);
    }

    public string Log(int cursoId, string repoPath)
    {
        return _gitAdapter.Log(repoPath);
    }
}
