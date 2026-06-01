namespace PyStudentIDE.Domain.Entities;

public class CasoPrueba
{
    public int IdCasoPrueba { get; set; }
    public int IdAsignacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Entrada { get; set; }
    public string SalidaEsperada { get; set; } = string.Empty;
    public int? Puntaje { get; set; }
    public bool Activo { get; set; }
}
