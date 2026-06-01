namespace PyStudentIDE.Application.DTOs;

public class CreateCursoRequest
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
}

public class CursoResponse
{
    public int IdCurso { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string? TipoParticipacion { get; set; }
}

public class JoinCursoRequest
{
    public int IdUsuario { get; set; }
    public string TipoParticipacion { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int IdRol { get; set; }
}

public class UpdateAssignmentDTO
{
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime? FechaLimite { get; set; }
    public bool? AdmiteTrabajoGrupal { get; set; }
    public int? TamanoMaximoGrupo { get; set; }
    public DateTime? InicioPeriodoPrueba { get; set; }
    public DateTime? FinPeriodoPrueba { get; set; }
}
