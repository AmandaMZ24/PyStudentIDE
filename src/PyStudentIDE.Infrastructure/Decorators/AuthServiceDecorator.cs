using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Services;
using PyStudentIDE.Infrastructure.Logging;

namespace PyStudentIDE.Infrastructure.Decorators;

public class AuthServiceDecorator : IAuthService
{
    private readonly IAuthService _inner;
    private readonly EventLogger _logger;

    public AuthServiceDecorator(IAuthService inner, EventLogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public LoginResponse Login(LoginRequest request)
    {
        try
        {
            var result = _inner.Login(request);
            if (result != null)
                _logger.Log(new LogEntry { Level = LogLevel.Info, Category = "AUTH", Message = $"Login exitoso: {request.Email}", Metadata = $"UserId={result.UsuarioId}" });
            else
                _logger.Log(new LogEntry { Level = LogLevel.Warning, Category = "AUTH", Message = $"Login fallido: {request.Email}" });
            return result!;
        }
        catch (Exception ex)
        {
            _logger.Log(new LogEntry { Level = LogLevel.Error, Category = "AUTH", Message = $"Error en login: {ex.Message}" });
            throw;
        }
    }

    public LoginResponse Register(RegisterRequest request)
    {
        try
        {
            var result = _inner.Register(request);
            if (result != null)
                _logger.Log(new LogEntry { Level = LogLevel.Info, Category = "AUTH", Message = $"Registro exitoso: {request.Email}", Metadata = $"UserId={result.UsuarioId}" });
            else
                _logger.Log(new LogEntry { Level = LogLevel.Warning, Category = "AUTH", Message = $"Registro fallido (email duplicado): {request.Email}" });
            return result!;
        }
        catch (Exception ex)
        {
            _logger.Log(new LogEntry { Level = LogLevel.Error, Category = "AUTH", Message = $"Error en registro: {ex.Message}" });
            throw;
        }
    }

    public bool ValidateToken(string token) => _inner.ValidateToken(token);
}
