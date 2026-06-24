using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.Application.Services;

public interface ILlaveCursoService
{
    GenerarLlaveResponse GenerarLlave(int idCurso);
    int RegistrarLlave(RegistrarLlaveRequest request);
    LlaveCursoResponse? ObtenerLlaveActiva(int idCurso);
    string FirmarContenido(byte[] contenido, int idCurso);
    bool VerificarFirma(byte[] contenido, string firma, int idCurso);
    void DesactivarLlave(int idLlave);
}
