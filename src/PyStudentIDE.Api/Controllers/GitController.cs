using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PyStudentIDE.Application.Facade;

namespace PyStudentIDE.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GitController : ControllerBase
{
    private readonly PyStudentFacade _facade;

    public GitController(PyStudentFacade facade) { _facade = facade; }

    [HttpPost("init")]
    public IActionResult Init([FromBody] GitRequest request)
    {
        try
        {
            var result = _facade.GitInit(request.CursoId, request.RepoPath);
            return Ok(new { output = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("add")]
    public IActionResult Add([FromBody] GitAddRequest request)
    {
        try
        {
            var result = _facade.GitAdd(request.CursoId, request.RepoPath, request.FilePattern);
            return Ok(new { output = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("commit")]
    public IActionResult Commit([FromBody] GitCommitRequest request)
    {
        try
        {
            var result = _facade.GitCommit(request.CursoId, request.RepoPath, request.Message, request.EntregaId);
            return Ok(new { output = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("push")]
    public IActionResult Push([FromBody] GitRequest request)
    {
        try
        {
            var result = _facade.GitPush(request.CursoId, request.RepoPath);
            return Ok(new { output = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("pull")]
    public IActionResult Pull([FromBody] GitRequest request)
    {
        try
        {
            var result = _facade.GitPull(request.CursoId, request.RepoPath);
            return Ok(new { output = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("status")]
    public IActionResult Status([FromQuery] int cursoId, [FromQuery] string repoPath)
    {
        try
        {
            var result = _facade.GitStatus(cursoId, repoPath);
            return Ok(new { output = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("log")]
    public IActionResult Log([FromQuery] int cursoId, [FromQuery] string repoPath)
    {
        try
        {
            var result = _facade.GitLog(cursoId, repoPath);
            return Ok(new { output = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class GitRequest
{
    public int CursoId { get; set; }
    public string RepoPath { get; set; } = string.Empty;
}

public class GitAddRequest
{
    public int CursoId { get; set; }
    public string RepoPath { get; set; } = string.Empty;
    public string FilePattern { get; set; } = ".";
}

public class GitCommitRequest
{
    public int CursoId { get; set; }
    public string RepoPath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int EntregaId { get; set; }
}
