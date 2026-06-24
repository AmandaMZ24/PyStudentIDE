using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Facade;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CursosController : ControllerBase
{
    private readonly PyStudentFacade _facade;

    public CursosController(PyStudentFacade facade) { _facade = facade; }

    [HttpPost]
    public IActionResult Create([FromBody] CreateCursoRequest request)
    {
        var docenteId = GetUserId();
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

    [HttpGet("mios")]
    public IActionResult GetMyCourses()
    {
        var usuarioId = GetUserId();
        var cursos = _facade.GetMyCourses(usuarioId);
        return Ok(cursos);
    }

    [HttpGet("{id}/estudiantes")]
    public IActionResult GetStudents(int id)
    {
        var estudiantes = _facade.GetStudentsByCourse(id);
        return Ok(estudiantes);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
