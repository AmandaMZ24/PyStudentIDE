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

        if (!asignacion.Activa)
            return new DeliveryResultDTO { Exitoso = false, Mensaje = "La asignación no está activa" };

        if (EstaEnPeriodoPrueba(asignacion))
            return new DeliveryResultDTO { Exitoso = false, Mensaje = "La entrega no está habilitada durante el período de prueba. Espere al período oficial de entrega." };

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
            TamanoBytes = fileBytes.Length,
            FechaCarga = DateTime.UtcNow,
            Contenido = dto.ContenidoBase64
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
            IdEntrega = entrega.IdEntrega,
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

        if (!asignacion.Activa)
            return new DeliveryResultDTO { Exitoso = false, Mensaje = "La asignación no está activa" };

        if (EstaEnPeriodoPrueba(asignacion))
            return new DeliveryResultDTO { Exitoso = false, Mensaje = "No se puede re-entregar durante el período de prueba" };

        var esTardia = DateTime.UtcNow > asignacion.FechaLimite;

        if (entregaAnterior != null && intento == 1)
        {
            var archivosOriginales = _unitOfWork.Repository<Archivo>().GetAll()
                .Where(a => a.IdEntrega == entregaAnteriorId)
                .ToList();

            foreach (var archivoOrig in archivosOriginales)
            {
                archivoOrig.VersionAnterior = $"SNAPSHOT_{entregaAnterior.FechaEntrega:yyyyMMddHHmmss}";
                _unitOfWork.Repository<Archivo>().Update(archivoOrig);
            }
            _unitOfWork.SaveChanges();
        }

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
            TamanoBytes = fileBytes.Length,
            FechaCarga = DateTime.UtcNow,
            VersionAnterior = entregaAnterior?.FirmaDigital,
            Contenido = dto.ContenidoBase64
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
            IdEntrega = entrega.IdEntrega,
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

    public IEnumerable<DeliveryDTO> GetDeliveriesByAssignment(int asignacionId, int estudianteId)
    {
        return _unitOfWork.Repository<Entrega>().GetAll()
            .Where(e => e.IdAsignacion == asignacionId && e.IdEstudiante == estudianteId)
            .Select(e => new DeliveryDTO
            {
                IdAsignacion = e.IdAsignacion,
                IdEstudiante = e.IdEstudiante,
                RutaArchivo = $"entrega_{e.NumeroIntento}.py",
                ContenidoBase64 = e.FirmaDigital ?? string.Empty
            })
            .ToList();
    }

    public IEnumerable<DeliveryResponse> GetDeliveriesByStudent(int estudianteId)
    {
        return _unitOfWork.Repository<Entrega>().GetAll()
            .Where(e => e.IdEstudiante == estudianteId)
            .Select(e => new DeliveryResponse
            {
                IdEntrega = e.IdEntrega,
                IdAsignacion = e.IdAsignacion,
                IdEstudiante = e.IdEstudiante,
                FechaEntrega = e.FechaEntrega,
                Estado = e.Estado,
                Calificacion = e.Calificacion,
                EsTardia = e.EsTardia,
                NumeroIntento = e.NumeroIntento,
                FirmaDigital = e.FirmaDigital
            })
            .ToList();
    }

    public IEnumerable<UsuarioResponse> GetStudentsByCourse(int cursoId)
    {
        return _unitOfWork.Repository<Matricula>().GetAll()
            .Where(m => m.IdCurso == cursoId && m.TipoParticipacion == "ESTUDIANTE")
            .Join(_unitOfWork.Repository<Usuario>().GetAll(),
                m => m.IdUsuario,
                u => u.IdUsuario,
                (m, u) => new UsuarioResponse
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    IdRol = u.IdRol
                })
            .ToList();
    }

    public IEnumerable<DeliveryResponse> GetDeliveriesByAssignmentAll(int asignacionId)
    {
        return _unitOfWork.Repository<Entrega>().GetAll()
            .Where(e => e.IdAsignacion == asignacionId)
            .Select(e => new DeliveryResponse
            {
                IdEntrega = e.IdEntrega,
                IdAsignacion = e.IdAsignacion,
                IdEstudiante = e.IdEstudiante,
                FechaEntrega = e.FechaEntrega,
                Estado = e.Estado,
                Calificacion = e.Calificacion,
                EsTardia = e.EsTardia,
                NumeroIntento = e.NumeroIntento,
                FirmaDigital = e.FirmaDigital
            })
            .ToList();
    }

    public string? GetDeliveryFileContent(int entregaId)
    {
        var archivo = _unitOfWork.Repository<Archivo>().GetAll()
            .FirstOrDefault(a => a.IdEntrega == entregaId);
        return archivo?.Contenido;
    }

    private static bool EstaEnPeriodoPrueba(Asignacion asignacion)
    {
        var ahora = DateTime.UtcNow;
        return asignacion.InicioPeriodoPrueba.HasValue
            && asignacion.FinPeriodoPrueba.HasValue
            && ahora >= asignacion.InicioPeriodoPrueba.Value
            && ahora < asignacion.FinPeriodoPrueba.Value;
    }

    private static string ComputeSHA256(byte[] content)
    {
        var hash = SHA256.HashData(content);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
