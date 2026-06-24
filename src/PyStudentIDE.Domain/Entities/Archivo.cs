namespace PyStudentIDE.Domain.Entities;

public class Archivo
{
    public int IdArchivo { get; set; }
    public int IdEntrega { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string TipoArchivo { get; set; } = string.Empty;
    public int TamanoBytes { get; set; }
    public DateTime FechaCarga { get; set; }
    public string? VersionAnterior { get; set; }
    public string? Contenido { get; set; }
}
