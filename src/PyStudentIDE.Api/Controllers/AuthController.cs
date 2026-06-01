using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Services;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) { _authService = authService; }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _authService.Login(request);
        if (result == null)
            return Unauthorized(new { message = "Credenciales inválidas" });
        return Ok(result);
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var result = _authService.Register(request);
        if (result == null)
            return BadRequest(new { message = "El correo ya está registrado" });
        return Ok(result);
    }

    [HttpPost("validate")]
    public IActionResult ValidateToken([FromBody] string token)
    {
        var valid = _authService.ValidateToken(token);
        return Ok(new { valid });
    }
}
