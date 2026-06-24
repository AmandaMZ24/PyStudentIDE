using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.Application.Services;

public interface IAssignmentService
{
    int CreateAssignment(AssignmentDTO dto, int docenteId);
    IEnumerable<AssignmentDTO> GetAssignmentsByCourse(int cursoId);
    AssignmentDTO? GetAssignmentById(int id);
    DeliveryResultDTO RegisterDelivery(DeliveryDTO dto);
    DeliveryResultDTO ResubmitDelivery(DeliveryDTO dto, int entregaAnteriorId);
    void UpdateAssignment(int id, UpdateAssignmentDTO dto);
    IEnumerable<CursoResponse> GetCoursesByUser(int usuarioId);
    IEnumerable<DeliveryDTO> GetDeliveriesByAssignment(int asignacionId, int estudianteId);
    IEnumerable<DeliveryResponse> GetDeliveriesByStudent(int estudianteId);
    IEnumerable<UsuarioResponse> GetStudentsByCourse(int cursoId);
    IEnumerable<DeliveryResponse> GetDeliveriesByAssignmentAll(int asignacionId);
    string? GetDeliveryFileContent(int entregaId);
}
