namespace PyStudentIDE.Application.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
