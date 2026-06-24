using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Services;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EntregasController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public EntregasController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpPost]
    public IActionResult Submit([FromBody] DeliveryDTO dto)
    {
        var result = _assignmentService.RegisterDelivery(dto);
        return Ok(result);
    }

    [HttpPost("resubmit/{entregaAnteriorId}")]
    public IActionResult Resubmit(int entregaAnteriorId, [FromBody] DeliveryDTO dto)
    {
        var result = _assignmentService.ResubmitDelivery(dto, entregaAnteriorId);
        return Ok(result);
    }

    [HttpGet("asignacion/{asignacionId}/estudiante/{estudianteId}")]
    public IActionResult GetEntregasByAsignacion(int asignacionId, int estudianteId)
    {
        var entregas = _assignmentService.GetDeliveriesByAssignment(asignacionId, estudianteId);
        return Ok(entregas);
    }

    [HttpGet("estudiante/{id}")]
    public IActionResult GetEntregasByEstudiante(int id)
    {
        var entregas = _assignmentService.GetDeliveriesByStudent(id);
        return Ok(entregas);
    }

    [HttpGet("asignacion/{asignacionId}/all")]
    public IActionResult GetEntregasByAsignacionAll(int asignacionId)
    {
        var entregas = _assignmentService.GetDeliveriesByAssignmentAll(asignacionId);
        return Ok(entregas);
    }

    [HttpGet("{id}/archivo")]
    public IActionResult GetArchivo(int id)
    {
        var contenido = _assignmentService.GetDeliveryFileContent(id);
        if (contenido == null)
            return NotFound(new { message = "Archivo no encontrado" });
        return Ok(new { contenido });
    }
}
