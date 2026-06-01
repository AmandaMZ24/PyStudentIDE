using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Services;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
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
}
