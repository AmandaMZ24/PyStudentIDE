namespace PyStudentIDE.Domain.Entities;

public class CommitGit
{
    public int IdCommit { get; set; }
    public int IdEntrega { get; set; }
    public string HashCommit { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public string Rama { get; set; } = string.Empty;
    public DateTime FechaCommit { get; set; }
}
