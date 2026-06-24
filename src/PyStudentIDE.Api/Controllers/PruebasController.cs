using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.Facade;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Authorize(Policy = "EstudianteOrDocente")]
[Route("api/[controller]")]
public class PruebasController : ControllerBase
{
    private readonly PyStudentFacade _facade;

    public PruebasController(PyStudentFacade facade) { _facade = facade; }

    [HttpPost("ejecutar")]
    public IActionResult Ejecutar([FromBody] EjecutarPruebasRequest request)
    {
        try
        {
            var results = _facade.RunTests(request.AssignmentId, request.EntregaId, request.FilePath);
            return Ok(new
            {
                total = results.Count(),
                aprobadas = results.Count(r => r.Passed),
                fallidas = results.Count(r => !r.Passed),
                resultados = results
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class EjecutarPruebasRequest
{
    public int AssignmentId { get; set; }
    public int EntregaId { get; set; }
    public string FilePath { get; set; } = string.Empty;
}
