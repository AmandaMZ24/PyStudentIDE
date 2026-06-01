using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Facade;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CursosController : ControllerBase
{
    private readonly PyStudentFacade _facade;

    public CursosController(PyStudentFacade facade) { _facade = facade; }

    [HttpPost]
    public IActionResult Create([FromBody] CreateCursoRequest request, [FromHeader] int docenteId)
    {
        var curso = _facade.CreateCourse(request, docenteId);
        return Ok(curso);
    }

    [HttpPost("{id}/join")]
    public IActionResult Join(int id, [FromBody] JoinCursoRequest request)
    {
        _facade.JoinCourse(id, request);
        return Ok(new { message = "Matrícula exitosa" });
    }

    [HttpGet("usuario/{usuarioId}")]
    public IActionResult GetByUser(int usuarioId)
    {
        var cursos = _facade.GetMyCourses(usuarioId);
        return Ok(cursos);
    }
}
