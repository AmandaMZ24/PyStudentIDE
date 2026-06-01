using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.Application.Services;

public interface IAssignmentService
{
    int CreateAssignment(AssignmentDTO dto, int docenteId);
    IEnumerable<AssignmentDTO> GetAssignmentsByCourse(int cursoId);
    AssignmentDTO? GetAssignmentById(int id);
    DeliveryResultDTO RegisterDelivery(DeliveryDTO dto);
    DeliveryResultDTO ResubmitDelivery(DeliveryDTO dto, int entregaAnteriorId);
    void UpdateAssignment(int id, Application.DTOs.UpdateAssignmentDTO dto);
    IEnumerable<Application.DTOs.CursoResponse> GetCoursesByUser(int usuarioId);
}
