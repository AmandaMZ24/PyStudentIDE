using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
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

        var token = GenerateJwtToken(user, roleName);

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

        var token = GenerateJwtToken(user, roleName);

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
            var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(Usuario user, string roleName)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_secretKey);
        var key = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
            new Claim(ClaimTypes.Email, user.Correo),
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim(ClaimTypes.Role, roleName)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string ComputeSHA256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
