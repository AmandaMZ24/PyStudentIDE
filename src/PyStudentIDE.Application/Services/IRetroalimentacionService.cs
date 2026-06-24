using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.Application.Services;

public interface IRetroalimentacionService
{
    RetroalimentacionResponse Create(int docenteId, CreateRetroalimentacionDTO dto);
    IEnumerable<RetroalimentacionResponse> GetByEntrega(int idEntrega);
    RetroalimentacionResponse? Update(int id, string comentario, decimal? calificacion);
    bool Delete(int id);
}
