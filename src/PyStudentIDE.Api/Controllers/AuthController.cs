using Microsoft.AspNetCore.Authorization;
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

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _authService.Login(request);
        if (result == null)
            return Unauthorized(new { message = "Credenciales inválidas" });
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var result = _authService.Register(request);
        if (result == null)
            return BadRequest(new { message = "El correo ya está registrado" });
        return Ok(result);
    }

    [Authorize]
    [HttpPost("validate")]
    public IActionResult ValidateToken()
    {
        return Ok(new { valid = true });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            correo = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}
