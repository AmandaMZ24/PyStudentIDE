namespace PyStudentIDE.Application.DTOs;

public class RegistrarLlaveRequest
{
    public int IdCurso { get; set; }
    public string LlavePublicaXml { get; set; } = string.Empty;
    public string Algoritmo { get; set; } = "RSA-2048";
}

public class LlaveCursoResponse
{
    public int IdLlave { get; set; }
    public int IdCurso { get; set; }
    public string Algoritmo { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class GenerarLlaveResponse
{
    public int IdLlave { get; set; }
    public string LlavePublicaXml { get; set; } = string.Empty;
    public string LlavePrivadaXml { get; set; } = string.Empty;
    public string Algoritmo { get; set; } = "RSA-2048";
    public string Mensaje { get; set; } = string.Empty;
}
