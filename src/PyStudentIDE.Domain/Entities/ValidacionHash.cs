namespace PyStudentIDE.Domain.Entities;

public class ValidacionHash
{
    public int IdValidacion { get; set; }
    public int IdArchivo { get; set; }
    public string Algoritmo { get; set; } = string.Empty;
    public string HashCalculado { get; set; } = string.Empty;
    public bool Valido { get; set; }
    public DateTime FechaValidacion { get; set; }
}
