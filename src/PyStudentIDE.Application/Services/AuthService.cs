using System.Security.Cryptography;
using System.Text;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Domain.Entities;

namespace PyStudentIDE.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _secretKey;

    public AuthService(IUnitOfWork unitOfWork, string secretKey)
    {
        _unitOfWork = unitOfWork;
        _secretKey = secretKey;
    }

    public LoginResponse Login(LoginRequest request)
    {
        var users = _unitOfWork.Repository<Usuario>().GetAll();
        var user = users.FirstOrDefault(u => u.Correo == request.Email);
        if (user == null) return null!;

        var passwordHash = ComputeSHA256(request.Password);
        if (user.PasswordHash != passwordHash) return null!;

        var role = _unitOfWork.Repository<Rol>().GetAll()
            .FirstOrDefault(r => r.IdRol == user.IdRol);
        var roleName = role?.Nombre ?? "ESTUDIANTE";

        var token = GenerateToken(user, roleName);

        return new LoginResponse
        {
            Token = token,
            Rol = roleName,
            UsuarioId = user.IdUsuario,
            Nombre = user.Nombre
        };
    }

    public LoginResponse Register(RegisterRequest request)
    {
        var existing = _unitOfWork.Repository<Usuario>().GetAll()
            .FirstOrDefault(u => u.Correo == request.Email);
        if (existing != null) return null!;

        var user = new Usuario
        {
            IdRol = request.IdRol,
            Nombre = request.Nombre,
            Correo = request.Email,
            PasswordHash = ComputeSHA256(request.Password),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _unitOfWork.Repository<Usuario>().Add(user);
        _unitOfWork.SaveChanges();

        var role = _unitOfWork.Repository<Rol>().GetAll()
            .FirstOrDefault(r => r.IdRol == user.IdRol);
        var roleName = role?.Nombre ?? "ESTUDIANTE";

        var token = GenerateToken(user, roleName);

        return new LoginResponse
        {
            Token = token,
            Rol = roleName,
            UsuarioId = user.IdUsuario,
            Nombre = user.Nombre
        };
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var bytes = Convert.FromBase64String(token);
            var decoded = Encoding.UTF8.GetString(bytes);
            var parts = decoded.Split('|');
            if (parts.Length != 5) return false;

            var payload = $"{parts[0]}|{parts[1]}|{parts[2]}|{parts[3]}";
            var expectedHmac = ComputeHMAC(payload);

            if (!string.Equals(parts[4], expectedHmac, StringComparison.OrdinalIgnoreCase))
                return false;

            var expirationTicks = long.Parse(parts[3]);
            var expiration = new DateTime(expirationTicks, DateTimeKind.Utc);
            return expiration > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateToken(Usuario user, string roleName)
    {
        var expiration = DateTime.UtcNow.AddHours(8);
        var payload = $"{user.IdUsuario}|{user.Correo}|{roleName}|{expiration.Ticks}";
        var hmac = ComputeHMAC(payload);
        var tokenString = $"{payload}|{hmac}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenString));
    }

    private string ComputeHMAC(string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ComputeSHA256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
