namespace PyStudentIDE.Domain.Entities;

public class Entrega
{
    public int IdEntrega { get; set; }
    public int IdAsignacion { get; set; }
    public int IdEstudiante { get; set; }
    public DateTime FechaEntrega { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal? Calificacion { get; set; }
    public bool EsTardia { get; set; }
    public int NumeroIntento { get; set; }
    public string? FirmaDigital { get; set; }
}
