namespace PyStudentIDE.Application.DTOs;

public class AssignmentDTO
{
    public int IdAsignacion { get; set; }
    public int IdCurso { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaLimite { get; set; }
    public bool AdmiteTrabajoGrupal { get; set; }
    public int? TamanoMaximoGrupo { get; set; }
    public DateTime? InicioPeriodoPrueba { get; set; }
    public DateTime? FinPeriodoPrueba { get; set; }
}

public class DeliveryDTO
{
    public int IdAsignacion { get; set; }
    public int IdEstudiante { get; set; }
    public string RutaArchivo { get; set; } = string.Empty;
    public string ContenidoBase64 { get; set; } = string.Empty;
}

public class DeliveryResultDTO
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? FirmaDigital { get; set; }
    public DateTime Timestamp { get; set; }
    public bool EsTardia { get; set; }
}
