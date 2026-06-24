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
    private readonly ILlaveCursoService _llaveCursoService;
    private readonly IGitService _gitService;
    private readonly IUnitOfWork _unitOfWork;

    public PyStudentFacade(
        IAuthService authService,
        IAssignmentService assignmentService,
        ITestEngine testEngine,
        IHashService hashService,
        ILlaveCursoService llaveCursoService,
        IGitService gitService,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _assignmentService = assignmentService;
        _testEngine = testEngine;
        _hashService = hashService;
        _llaveCursoService = llaveCursoService;
        _gitService = gitService;
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

    public GenerarLlaveResponse GenerarLlaveCurso(int idCurso) =>
        _llaveCursoService.GenerarLlave(idCurso);

    public int RegistrarLlaveCurso(RegistrarLlaveRequest request) =>
        _llaveCursoService.RegistrarLlave(request);

    public LlaveCursoResponse? ObtenerLlaveActiva(int idCurso) =>
        _llaveCursoService.ObtenerLlaveActiva(idCurso);

    public void DesactivarLlave(int idLlave) =>
        _llaveCursoService.DesactivarLlave(idLlave);

    public string GitInit(int cursoId, string repoPath) =>
        _gitService.Init(cursoId, repoPath);

    public string GitClone(int cursoId, string url, string localPath) =>
        _gitService.Clone(cursoId, url, localPath);

    public string GitAdd(int cursoId, string repoPath, string filePattern) =>
        _gitService.Add(cursoId, repoPath, filePattern);

    public string GitCommit(int cursoId, string repoPath, string message, int entregaId = 0) =>
        _gitService.Commit(cursoId, repoPath, message, entregaId);

    public string GitPush(int cursoId, string repoPath) =>
        _gitService.Push(cursoId, repoPath);

    public string GitPull(int cursoId, string repoPath) =>
        _gitService.Pull(cursoId, repoPath);

    public string GitStatus(int cursoId, string repoPath) =>
        _gitService.Status(cursoId, repoPath);

    public string GitLog(int cursoId, string repoPath) =>
        _gitService.Log(cursoId, repoPath);

    public IEnumerable<UsuarioResponse> GetStudentsByCourse(int cursoId) =>
        _assignmentService.GetStudentsByCourse(cursoId);

    public IEnumerable<DeliveryResponse> GetDeliveriesByAssignmentAll(int asignacionId) =>
        _assignmentService.GetDeliveriesByAssignmentAll(asignacionId);
}
