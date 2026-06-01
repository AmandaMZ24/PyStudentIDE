using System.Security.Cryptography;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Domain.Entities;

namespace PyStudentIDE.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public int CreateAssignment(AssignmentDTO dto, int docenteId)
    {
        var asignacion = new Asignacion
        {
            IdCurso = dto.IdCurso,
            IdDocente = docenteId,
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            FechaPublicacion = DateTime.UtcNow,
            FechaLimite = dto.FechaLimite,
            Activa = true,
            AdmiteTrabajoGrupal = dto.AdmiteTrabajoGrupal,
            TamanoMaximoGrupo = dto.TamanoMaximoGrupo,
            InicioPeriodoPrueba = dto.InicioPeriodoPrueba,
            FinPeriodoPrueba = dto.FinPeriodoPrueba
        };

        _unitOfWork.Repository<Asignacion>().Add(asignacion);
        _unitOfWork.SaveChanges();
        return asignacion.IdAsignacion;
    }

    public IEnumerable<AssignmentDTO> GetAssignmentsByCourse(int cursoId)
    {
        return _unitOfWork.Repository<Asignacion>().GetAll()
            .Where(a => a.IdCurso == cursoId)
            .Select(a => new AssignmentDTO
            {
                IdAsignacion = a.IdAsignacion,
                IdCurso = a.IdCurso,
                Titulo = a.Titulo,
                Descripcion = a.Descripcion,
                FechaLimite = a.FechaLimite,
                AdmiteTrabajoGrupal = a.AdmiteTrabajoGrupal,
                TamanoMaximoGrupo = a.TamanoMaximoGrupo,
                InicioPeriodoPrueba = a.InicioPeriodoPrueba,
                FinPeriodoPrueba = a.FinPeriodoPrueba
            })
            .ToList();
    }

    public AssignmentDTO? GetAssignmentById(int id)
    {
        var a = _unitOfWork.Repository<Asignacion>().GetById(id);
        if (a == null) return null;

        return new AssignmentDTO
        {
            IdAsignacion = a.IdAsignacion,
            IdCurso = a.IdCurso,
            Titulo = a.Titulo,
            Descripcion = a.Descripcion,
            FechaLimite = a.FechaLimite,
            AdmiteTrabajoGrupal = a.AdmiteTrabajoGrupal,
            TamanoMaximoGrupo = a.TamanoMaximoGrupo,
            InicioPeriodoPrueba = a.InicioPeriodoPrueba,
            FinPeriodoPrueba = a.FinPeriodoPrueba
        };
    }

    public DeliveryResultDTO RegisterDelivery(DeliveryDTO dto)
    {
        var asignacion = _unitOfWork.Repository<Asignacion>().GetById(dto.IdAsignacion);
        if (asignacion == null)
            return new DeliveryResultDTO { Exitoso = false, Mensaje = "Asignación no encontrada" };

        var esTardia = DateTime.UtcNow > asignacion.FechaLimite;

        var entrega = new Entrega
        {
            IdAsignacion = dto.IdAsignacion,
            IdEstudiante = dto.IdEstudiante,
            FechaEntrega = DateTime.UtcNow,
            Estado = "RECIBIDA",
            EsTardia = esTardia,
            NumeroIntento = 1
        };

        _unitOfWork.Repository<Entrega>().Add(entrega);
        _unitOfWork.SaveChanges();

        var fileBytes = Convert.FromBase64String(dto.ContenidoBase64);

        var archivo = new Archivo
        {
            IdEntrega = entrega.IdEntrega,
            NombreArchivo = Path.GetFileName(dto.RutaArchivo),
            RutaArchivo = dto.RutaArchivo,
            TipoArchivo = Path.GetExtension(dto.RutaArchivo).TrimStart('.'),
            TamanioBytes = fileBytes.Length,
            FechaCarga = DateTime.UtcNow
        };

        _unitOfWork.Repository<Archivo>().Add(archivo);
        _unitOfWork.SaveChanges();

        var hash = ComputeSHA256(fileBytes);

        var validacion = new ValidacionHash
        {
            IdArchivo = archivo.IdArchivo,
            Algoritmo = "SHA-256",
            HashCalculado = hash,
            Valido = true,
            FechaValidacion = DateTime.UtcNow
        };

        _unitOfWork.Repository<ValidacionHash>().Add(validacion);
        _unitOfWork.SaveChanges();

        entrega.FirmaDigital = hash;
        _unitOfWork.Repository<Entrega>().Update(entrega);
        _unitOfWork.SaveChanges();

        return new DeliveryResultDTO
        {
            Exitoso = true,
            Mensaje = "Entrega registrada exitosamente",
            FirmaDigital = hash,
            Timestamp = entrega.FechaEntrega,
            EsTardia = esTardia
        };
    }

    public DeliveryResultDTO ResubmitDelivery(DeliveryDTO dto, int entregaAnteriorId)
    {
        var entregaAnterior = _unitOfWork.Repository<Entrega>().GetById(entregaAnteriorId);
        var intento = entregaAnterior?.NumeroIntento ?? 0;

        var asignacion = _unitOfWork.Repository<Asignacion>().GetById(dto.IdAsignacion);
        if (asignacion == null)
            return new DeliveryResultDTO { Exitoso = false, Mensaje = "Asignación no encontrada" };

        var esTardia = DateTime.UtcNow > asignacion.FechaLimite;

        var entrega = new Entrega
        {
            IdAsignacion = dto.IdAsignacion,
            IdEstudiante = dto.IdEstudiante,
            FechaEntrega = DateTime.UtcNow,
            Estado = "RECIBIDA",
            EsTardia = esTardia,
            NumeroIntento = intento + 1
        };

        _unitOfWork.Repository<Entrega>().Add(entrega);
        _unitOfWork.SaveChanges();

        var fileBytes = Convert.FromBase64String(dto.ContenidoBase64);

        var archivo = new Archivo
        {
            IdEntrega = entrega.IdEntrega,
            NombreArchivo = Path.GetFileName(dto.RutaArchivo),
            RutaArchivo = dto.RutaArchivo,
            TipoArchivo = Path.GetExtension(dto.RutaArchivo).TrimStart('.'),
            TamanioBytes = fileBytes.Length,
            FechaCarga = DateTime.UtcNow,
            VersionAnterior = entregaAnterior?.FirmaDigital
        };

        _unitOfWork.Repository<Archivo>().Add(archivo);
        _unitOfWork.SaveChanges();

        var hash = ComputeSHA256(fileBytes);

        var validacion = new ValidacionHash
        {
            IdArchivo = archivo.IdArchivo,
            Algoritmo = "SHA-256",
            HashCalculado = hash,
            Valido = true,
            FechaValidacion = DateTime.UtcNow
        };

        _unitOfWork.Repository<ValidacionHash>().Add(validacion);
        _unitOfWork.SaveChanges();

        entrega.FirmaDigital = hash;
        _unitOfWork.Repository<Entrega>().Update(entrega);
        _unitOfWork.SaveChanges();

        return new DeliveryResultDTO
        {
            Exitoso = true,
            Mensaje = $"Re-entrega registrada (intento {intento + 1})",
            FirmaDigital = hash,
            Timestamp = entrega.FechaEntrega,
            EsTardia = esTardia
        };
    }

    public void UpdateAssignment(int id, UpdateAssignmentDTO dto)
    {
        var a = _unitOfWork.Repository<Asignacion>().GetById(id);
        if (a == null) return;

        if (dto.Titulo != null) a.Titulo = dto.Titulo;
        if (dto.Descripcion != null) a.Descripcion = dto.Descripcion;
        if (dto.FechaLimite.HasValue) a.FechaLimite = dto.FechaLimite.Value;
        if (dto.AdmiteTrabajoGrupal.HasValue) a.AdmiteTrabajoGrupal = dto.AdmiteTrabajoGrupal.Value;
        if (dto.TamanoMaximoGrupo.HasValue) a.TamanoMaximoGrupo = dto.TamanoMaximoGrupo;
        if (dto.InicioPeriodoPrueba.HasValue) a.InicioPeriodoPrueba = dto.InicioPeriodoPrueba;
        if (dto.FinPeriodoPrueba.HasValue) a.FinPeriodoPrueba = dto.FinPeriodoPrueba;

        _unitOfWork.Repository<Asignacion>().Update(a);
        _unitOfWork.SaveChanges();
    }

    public IEnumerable<CursoResponse> GetCoursesByUser(int usuarioId)
    {
        var matriculas = _unitOfWork.Repository<Matricula>().GetAll()
            .Where(m => m.IdUsuario == usuarioId)
            .ToList();

        var cursos = _unitOfWork.Repository<Curso>().GetAll().ToList();

        return matriculas.Join(cursos,
                m => m.IdCurso,
                c => c.IdCurso,
                (m, c) => new CursoResponse
                {
                    IdCurso = c.IdCurso,
                    Codigo = c.Codigo,
                    Nombre = c.Nombre,
                    Periodo = c.Periodo,
                    Activo = c.Activo,
                    TipoParticipacion = m.TipoParticipacion
                })
            .ToList();
    }

    private static string ComputeSHA256(byte[] content)
    {
        var hash = SHA256.HashData(content);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
