using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Services;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AsignacionesController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AsignacionesController(IAssignmentService assignmentService) { _assignmentService = assignmentService; }

    [HttpPost]
    public IActionResult Create([FromBody] AssignmentDTO dto, [FromHeader] int docenteId)
    {
        var id = _assignmentService.CreateAssignment(dto, docenteId);
        return Ok(new { id });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateAssignmentDTO dto)
    {
        _assignmentService.UpdateAssignment(id, dto);
        return Ok(new { message = "Asignación actualizada" });
    }

    [HttpGet("curso/{cursoId}")]
    public IActionResult GetByCourse(int cursoId)
    {
        var asignaciones = _assignmentService.GetAssignmentsByCourse(cursoId);
        return Ok(asignaciones);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var asignacion = _assignmentService.GetAssignmentById(id);
        if (asignacion == null) return NotFound();
        return Ok(asignacion);
    }
}
