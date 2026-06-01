namespace PyStudentIDE.Domain.Entities;

public class Curso
{
    public int IdCurso { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
