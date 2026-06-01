namespace PyStudentIDE.Domain.Entities;

public class ResultadoPrueba
{
    public int IdResultado { get; set; }
    public int IdEntrega { get; set; }
    public int IdCasoPrueba { get; set; }
    public bool Aprobado { get; set; }
    public string? SalidaObtenida { get; set; }
    public string? MensajeError { get; set; }
    public decimal? TiempoEjecucion { get; set; }
    public DateTime FechaEjecucion { get; set; }
}
