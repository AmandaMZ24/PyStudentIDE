using PyStudentIDE.Application.DTOs;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Application.Services;
using PyStudentIDE.Domain.Entities;

namespace PyStudentIDE.Application.Facade;

public class PyStudentFacade
{
    private readonly IAuthService _authService;
    private readonly IAssignmentService _assignmentService;
    private readonly ITestEngine _testEngine;
    private readonly IHashService _hashService;
    private readonly IUnitOfWork _unitOfWork;

    public PyStudentFacade(
        IAuthService authService,
        IAssignmentService assignmentService,
        ITestEngine testEngine,
        IHashService hashService,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _assignmentService = assignmentService;
        _testEngine = testEngine;
        _hashService = hashService;
        _unitOfWork = unitOfWork;
    }

    public LoginResponse Login(LoginRequest request) => _authService.Login(request);

    public LoginResponse Register(RegisterRequest request) => _authService.Register(request);

    public CursoResponse CreateCourse(CreateCursoRequest request, int docenteId)
    {
        var curso = new Curso
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Periodo = request.Periodo,
            Activo = true
        };

        _unitOfWork.Repository<Curso>().Add(curso);
        _unitOfWork.SaveChanges();

        var matricula = new Matricula
        {
            IdUsuario = docenteId,
            IdCurso = curso.IdCurso,
            TipoParticipacion = "DOCENTE",
            FechaMatricula = DateTime.UtcNow
        };

        _unitOfWork.Repository<Matricula>().Add(matricula);
        _unitOfWork.SaveChanges();

        return new CursoResponse
        {
            IdCurso = curso.IdCurso,
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Periodo = curso.Periodo,
            Activo = curso.Activo,
            TipoParticipacion = "DOCENTE"
        };
    }

    public void JoinCourse(int cursoId, JoinCursoRequest request)
    {
        var matricula = new Matricula
        {
            IdUsuario = request.IdUsuario,
            IdCurso = cursoId,
            TipoParticipacion = request.TipoParticipacion,
            FechaMatricula = DateTime.UtcNow
        };

        _unitOfWork.Repository<Matricula>().Add(matricula);
        _unitOfWork.SaveChanges();
    }

    public IEnumerable<CursoResponse> GetMyCourses(int usuarioId) =>
        _assignmentService.GetCoursesByUser(usuarioId);

    public int CreateAssignment(AssignmentDTO dto, int docenteId) =>
        _assignmentService.CreateAssignment(dto, docenteId);

    public void UpdateAssignment(int id, UpdateAssignmentDTO dto) =>
        _assignmentService.UpdateAssignment(id, dto);

    public IEnumerable<AssignmentDTO> GetCourseAssignments(int cursoId) =>
        _assignmentService.GetAssignmentsByCourse(cursoId);

    public AssignmentDTO? GetAssignmentById(int id) =>
        _assignmentService.GetAssignmentById(id);

    public DeliveryResultDTO SubmitScript(DeliveryDTO dto) =>
        _assignmentService.RegisterDelivery(dto);

    public DeliveryResultDTO ResubmitScript(DeliveryDTO dto, int entregaAnteriorId) =>
        _assignmentService.ResubmitDelivery(dto, entregaAnteriorId);

    public IEnumerable<TestResult> RunTests(int assignmentId, int entregaId, string filePath) =>
        _testEngine.ExecuteTests(assignmentId, entregaId, filePath);
}
