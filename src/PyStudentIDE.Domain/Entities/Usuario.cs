namespace PyStudentIDE.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
