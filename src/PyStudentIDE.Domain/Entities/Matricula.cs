namespace PyStudentIDE.Domain.Entities;

public class Matricula
{
    public int IdMatricula { get; set; }
    public int IdUsuario { get; set; }
    public int IdCurso { get; set; }
    public string TipoParticipacion { get; set; } = string.Empty;
    public DateTime FechaMatricula { get; set; }
}
