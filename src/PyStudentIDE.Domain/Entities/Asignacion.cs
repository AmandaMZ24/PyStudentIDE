namespace PyStudentIDE.Domain.Entities;

public class Asignacion
{
    public int IdAsignacion { get; set; }
    public int IdCurso { get; set; }
    public int IdDocente { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public DateTime FechaLimite { get; set; }
    public bool Activa { get; set; }
    public bool AdmiteTrabajoGrupal { get; set; }
    public int? TamanoMaximoGrupo { get; set; }
    public DateTime? InicioPeriodoPrueba { get; set; }
    public DateTime? FinPeriodoPrueba { get; set; }
}
