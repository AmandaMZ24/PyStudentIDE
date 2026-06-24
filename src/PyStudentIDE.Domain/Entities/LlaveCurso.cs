namespace PyStudentIDE.Domain.Entities;

public class LlaveCurso
{
    public int IdLlave { get; set; }
    public int IdCurso { get; set; }
    public string LlavePublicaXml { get; set; } = string.Empty;
    public string Algoritmo { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public DateTime FechaCreacion { get; set; }
}
