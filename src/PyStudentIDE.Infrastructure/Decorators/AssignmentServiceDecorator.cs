using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Services;
using PyStudentIDE.Infrastructure.Logging;

namespace PyStudentIDE.Infrastructure.Decorators;

public class AssignmentServiceDecorator : IAssignmentService
{
    private readonly IAssignmentService _inner;
    private readonly EventLogger _logger;

    public AssignmentServiceDecorator(IAssignmentService inner, EventLogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public int CreateAssignment(AssignmentDTO dto, int docenteId)
    {
        try
        {
            var id = _inner.CreateAssignment(dto, docenteId);
            _logger.Log(new LogEntry { Level = LogLevel.Info, Category = "ASSIGNMENT", Message = $"Asignación creada: {dto.Titulo}", Metadata = $"AssignmentId={id}, DocenteId={docenteId}" });
            return id;
        }
        catch (Exception ex)
        {
            _logger.Log(new LogEntry { Level = LogLevel.Error, Category = "ASSIGNMENT", Message = $"Error creando asignación: {ex.Message}" });
            throw;
        }
    }

    public IEnumerable<AssignmentDTO> GetAssignmentsByCourse(int cursoId)
    {
        return _inner.GetAssignmentsByCourse(cursoId);
    }

    public AssignmentDTO? GetAssignmentById(int id)
    {
        return _inner.GetAssignmentById(id);
    }

    public DeliveryResultDTO RegisterDelivery(DeliveryDTO dto)
    {
        try
        {
            var result = _inner.RegisterDelivery(dto);
            _logger.LogDelivery(dto.IdEstudiante, dto.IdAsignacion, result.FirmaDigital ?? "N/A");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Log(new LogEntry { Level = LogLevel.Error, Category = "DELIVERY", Message = $"Error registrando entrega: {ex.Message}" });
            throw;
        }
    }

    public DeliveryResultDTO ResubmitDelivery(DeliveryDTO dto, int entregaAnteriorId)
    {
        try
        {
            var result = _inner.ResubmitDelivery(dto, entregaAnteriorId);
            _logger.Log(new LogEntry { Level = LogLevel.Info, Category = "DELIVERY", Message = $"Re-entrega registrada para entrega anterior {entregaAnteriorId}", Metadata = $"AssignmentId={dto.IdAsignacion}, StudentId={dto.IdEstudiante}" });
            return result;
        }
        catch (Exception ex)
        {
            _logger.Log(new LogEntry { Level = LogLevel.Error, Category = "DELIVERY", Message = $"Error registrando re-entrega: {ex.Message}" });
            throw;
        }
    }

    public void UpdateAssignment(int id, UpdateAssignmentDTO dto)
    {
        try
        {
            _inner.UpdateAssignment(id, dto);
            _logger.Log(new LogEntry { Level = LogLevel.Info, Category = "ASSIGNMENT", Message = $"Asignación {id} actualizada" });
        }
        catch (Exception ex)
        {
            _logger.Log(new LogEntry { Level = LogLevel.Error, Category = "ASSIGNMENT", Message = $"Error actualizando asignación {id}: {ex.Message}" });
            throw;
        }
    }

    public IEnumerable<CursoResponse> GetCoursesByUser(int usuarioId)
    {
        return _inner.GetCoursesByUser(usuarioId);
    }
}
