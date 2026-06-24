namespace PyStudentIDE.Domain.Entities;

public class Retroalimentacion
{
    public int IdRetroalimentacion { get; set; }
    public int IdEntrega { get; set; }
    public int IdDocente { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public decimal? Calificacion { get; set; }
    public DateTime FechaCreacion { get; set; }
}
