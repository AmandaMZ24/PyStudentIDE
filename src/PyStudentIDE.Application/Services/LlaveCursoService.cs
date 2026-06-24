using System.Security.Cryptography;
using System.Text;
using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Domain.Entities;

namespace PyStudentIDE.Application.Services;

public class LlaveCursoService : ILlaveCursoService
{
    private readonly IUnitOfWork _unitOfWork;

    public LlaveCursoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public GenerarLlaveResponse GenerarLlave(int idCurso)
    {
        using var rsa = RSA.Create(2048);
        var publicKeyXml = rsa.ToXmlString(false);
        var privateKeyXml = rsa.ToXmlString(true);

        var llave = new LlaveCurso
        {
            IdCurso = idCurso,
            LlavePublicaXml = publicKeyXml,
            Algoritmo = "RSA-2048",
            Activa = true,
            FechaCreacion = DateTime.UtcNow
        };

        var repo = _unitOfWork.Repository<LlaveCurso>();
        var llavesActivas = repo.GetAll().Where(l => l.IdCurso == idCurso && l.Activa).ToList();
        foreach (var l in llavesActivas)
        {
            l.Activa = false;
            repo.Update(l);
        }

        repo.Add(llave);
        _unitOfWork.SaveChanges();

        return new GenerarLlaveResponse
        {
            IdLlave = llave.IdLlave,
            LlavePublicaXml = publicKeyXml,
            LlavePrivadaXml = privateKeyXml,
            Algoritmo = "RSA-2048",
            Mensaje = "Llave RSA-2048 generada exitosamente"
        };
    }

    public int RegistrarLlave(RegistrarLlaveRequest request)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(request.LlavePublicaXml);
        }
        catch
        {
            throw new InvalidOperationException("La llave pública proporcionada no es válida");
        }

        var repo = _unitOfWork.Repository<LlaveCurso>();
        var llavesActivas = repo.GetAll().Where(l => l.IdCurso == request.IdCurso && l.Activa).ToList();
        foreach (var l in llavesActivas)
        {
            l.Activa = false;
            repo.Update(l);
        }

        var llave = new LlaveCurso
        {
            IdCurso = request.IdCurso,
            LlavePublicaXml = request.LlavePublicaXml,
            Algoritmo = request.Algoritmo,
            Activa = true,
            FechaCreacion = DateTime.UtcNow
        };

        repo.Add(llave);
        _unitOfWork.SaveChanges();
        return llave.IdLlave;
    }

    public LlaveCursoResponse? ObtenerLlaveActiva(int idCurso)
    {
        var llave = _unitOfWork.Repository<LlaveCurso>().GetAll()
            .FirstOrDefault(l => l.IdCurso == idCurso && l.Activa);

        if (llave == null) return null;

        return new LlaveCursoResponse
        {
            IdLlave = llave.IdLlave,
            IdCurso = llave.IdCurso,
            Algoritmo = llave.Algoritmo,
            Activa = llave.Activa,
            FechaCreacion = llave.FechaCreacion
        };
    }

    public string FirmarContenido(byte[] contenido, int idCurso)
    {
        var llave = _unitOfWork.Repository<LlaveCurso>().GetAll()
            .FirstOrDefault(l => l.IdCurso == idCurso && l.Activa)
            ?? throw new InvalidOperationException("No hay una llave activa para este curso");

        using var rsa = RSA.Create();
        rsa.FromXmlString(llave.LlavePublicaXml);

        var hash = SHA256.HashData(contenido);
        var firma = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(firma);
    }

    public bool VerificarFirma(byte[] contenido, string firma, int idCurso)
    {
        var llave = _unitOfWork.Repository<LlaveCurso>().GetAll()
            .FirstOrDefault(l => l.IdCurso == idCurso && l.Activa);

        if (llave == null) return false;

        try
        {
            using var rsa = RSA.Create();
            rsa.FromXmlString(llave.LlavePublicaXml);

            var hash = SHA256.HashData(contenido);
            var firmaBytes = Convert.FromBase64String(firma);
            return rsa.VerifyHash(hash, firmaBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }

    public void DesactivarLlave(int idLlave)
    {
        var llave = _unitOfWork.Repository<LlaveCurso>().GetById(idLlave);
        if (llave == null) return;

        llave.Activa = false;
        _unitOfWork.Repository<LlaveCurso>().Update(llave);
        _unitOfWork.SaveChanges();
    }
}
