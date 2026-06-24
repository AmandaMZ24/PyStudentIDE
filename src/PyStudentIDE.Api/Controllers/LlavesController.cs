using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Facade;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LlavesController : ControllerBase
{
    private readonly PyStudentFacade _facade;

    public LlavesController(PyStudentFacade facade) { _facade = facade; }

    [Authorize(Policy = "Docente")]
    [HttpPost("generar/{cursoId}")]
    public IActionResult Generar(int cursoId)
    {
        var result = _facade.GenerarLlaveCurso(cursoId);
        return Ok(result);
    }

    [Authorize(Policy = "Docente")]
    [HttpPost("registrar")]
    public IActionResult Registrar([FromBody] RegistrarLlaveRequest request)
    {
        try
        {
            var id = _facade.RegistrarLlaveCurso(request);
            return Ok(new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "EstudianteOrDocente")]
    [HttpGet("activa/{cursoId}")]
    public IActionResult ObtenerActiva(int cursoId)
    {
        var llave = _facade.ObtenerLlaveActiva(cursoId);
        if (llave == null)
            return NotFound(new { message = "No hay una llave activa para este curso" });
        return Ok(llave);
    }

    [Authorize(Policy = "Docente")]
    [HttpPost("desactivar/{idLlave}")]
    public IActionResult Desactivar(int idLlave)
    {
        _facade.DesactivarLlave(idLlave);
        return Ok(new { message = "Llave desactivada" });
    }
}
