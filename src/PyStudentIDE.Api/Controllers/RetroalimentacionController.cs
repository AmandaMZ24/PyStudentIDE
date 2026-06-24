using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Services;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RetroalimentacionController : ControllerBase
{
    private readonly IRetroalimentacionService _service;

    public RetroalimentacionController(IRetroalimentacionService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateRetroalimentacionDTO dto)
    {
        try
        {
            var docenteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = _service.Create(docenteId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("entrega/{idEntrega}")]
    public IActionResult GetByEntrega(int idEntrega)
    {
        var result = _service.GetByEntrega(idEntrega);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateRetroalimentacionDTO dto)
    {
        var result = _service.Update(id, dto.Comentario, dto.Calificacion);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (_service.Delete(id)) return Ok(new { message = "Retroalimentación eliminada" });
        return NotFound();
    }
}

public class UpdateRetroalimentacionDTO
{
    public string Comentario { get; set; } = string.Empty;
    public decimal? Calificacion { get; set; }
}
