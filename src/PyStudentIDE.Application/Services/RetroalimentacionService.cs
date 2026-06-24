using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Domain.Entities;

namespace PyStudentIDE.Application.Services;

public class RetroalimentacionService : IRetroalimentacionService
{
    private readonly IUnitOfWork _unitOfWork;

    public RetroalimentacionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public RetroalimentacionResponse Create(int docenteId, CreateRetroalimentacionDTO dto)
    {
        var entrega = _unitOfWork.Repository<Entrega>().GetById(dto.IdEntrega);
        if (entrega == null)
            throw new InvalidOperationException("Entrega no encontrada");

        var entity = new Retroalimentacion
        {
            IdEntrega = dto.IdEntrega,
            IdDocente = docenteId,
            Comentario = dto.Comentario,
            Calificacion = dto.Calificacion,
            FechaCreacion = DateTime.UtcNow
        };

        _unitOfWork.Repository<Retroalimentacion>().Add(entity);
        _unitOfWork.SaveChanges();

        if (dto.Calificacion.HasValue)
        {
            entrega.Calificacion = dto.Calificacion;
            entrega.Estado = "CALIFICADA";
            _unitOfWork.Repository<Entrega>().Update(entrega);
            _unitOfWork.SaveChanges();
        }

        var docente = _unitOfWork.Repository<Usuario>().GetById(docenteId);

        return MapToResponse(entity, docente?.Nombre ?? "");
    }

    public IEnumerable<RetroalimentacionResponse> GetByEntrega(int idEntrega)
    {
        var entities = _unitOfWork.Repository<Retroalimentacion>().GetAll()
            .Where(r => r.IdEntrega == idEntrega)
            .ToList();

        var docentes = _unitOfWork.Repository<Usuario>().GetAll()
            .ToDictionary(u => u.IdUsuario, u => u.Nombre);

        return entities.Select(e => MapToResponse(e, docentes.GetValueOrDefault(e.IdDocente, ""))).ToList();
    }

    public RetroalimentacionResponse? Update(int id, string comentario, decimal? calificacion)
    {
        var entity = _unitOfWork.Repository<Retroalimentacion>().GetById(id);
        if (entity == null) return null;

        entity.Comentario = comentario;
        if (calificacion.HasValue)
            entity.Calificacion = calificacion;

        _unitOfWork.Repository<Retroalimentacion>().Update(entity);
        _unitOfWork.SaveChanges();

        var docente = _unitOfWork.Repository<Usuario>().GetById(entity.IdDocente);

        return MapToResponse(entity, docente?.Nombre ?? "");
    }

    public bool Delete(int id)
    {
        var entity = _unitOfWork.Repository<Retroalimentacion>().GetById(id);
        if (entity == null) return false;

        _unitOfWork.Repository<Retroalimentacion>().Delete(id);
        _unitOfWork.SaveChanges();
        return true;
    }

    private static RetroalimentacionResponse MapToResponse(Retroalimentacion entity, string nombreDocente)
    {
        return new RetroalimentacionResponse
        {
            IdRetroalimentacion = entity.IdRetroalimentacion,
            IdEntrega = entity.IdEntrega,
            IdDocente = entity.IdDocente,
            NombreDocente = nombreDocente,
            Comentario = entity.Comentario,
            Calificacion = entity.Calificacion,
            FechaCreacion = entity.FechaCreacion
        };
    }
}
