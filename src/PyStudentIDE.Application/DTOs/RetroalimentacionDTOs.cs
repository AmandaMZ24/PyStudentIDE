namespace PyStudentIDE.Application.DTOs;

public class CreateRetroalimentacionDTO
{
    public int IdEntrega { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public decimal? Calificacion { get; set; }
}

public class RetroalimentacionResponse
{
    public int IdRetroalimentacion { get; set; }
    public int IdEntrega { get; set; }
    public int IdDocente { get; set; }
    public string NombreDocente { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public decimal? Calificacion { get; set; }
    public DateTime FechaCreacion { get; set; }
}
