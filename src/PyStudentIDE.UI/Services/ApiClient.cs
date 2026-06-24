using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.UI.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http) { _http = http; }

    private void SetAuth()
    {
        if (!string.IsNullOrEmpty(App.CurrentToken))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", App.CurrentToken);
        }
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task<List<CursoResponse>> GetCoursesAsync(int usuarioId)
    {
        SetAuth();
        return await _http.GetFromJsonAsync<List<CursoResponse>>($"api/cursos/usuario/{usuarioId}") ?? new();
    }

    public async Task<CursoResponse?> CreateCourseAsync(CreateCursoRequest request, int docenteId)
    {
        SetAuth();
        var msg = new HttpRequestMessage(HttpMethod.Post, "api/cursos");
        msg.Headers.Add("docenteId", docenteId.ToString());
        msg.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(msg);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CursoResponse>();
    }

    public async Task<bool> JoinCourseAsync(int cursoId, JoinCursoRequest request)
    {
        SetAuth();
        var response = await _http.PostAsJsonAsync($"api/cursos/{cursoId}/join", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AssignmentDTO>> GetAssignmentsByCourseAsync(int cursoId)
    {
        SetAuth();
        return await _http.GetFromJsonAsync<List<AssignmentDTO>>($"api/asignaciones/curso/{cursoId}") ?? new();
    }

    public async Task<int?> CreateAssignmentAsync(AssignmentDTO dto, int docenteId)
    {
        SetAuth();
        var msg = new HttpRequestMessage(HttpMethod.Post, "api/asignaciones");
        msg.Headers.Add("docenteId", docenteId.ToString());
        msg.Content = JsonContent.Create(dto);
        var response = await _http.SendAsync(msg);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        return result?["id"];
    }

    public async Task<bool> UpdateAssignmentAsync(int id, UpdateAssignmentDTO dto)
    {
        SetAuth();
        var response = await _http.PutAsJsonAsync($"api/asignaciones/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<DeliveryResponse>> GetDeliveriesByStudentAsync(int estudianteId)
    {
        SetAuth();
        return await _http.GetFromJsonAsync<List<DeliveryResponse>>($"api/entregas/estudiante/{estudianteId}") ?? new();
    }

    public async Task<DeliveryResultDTO?> SubmitDeliveryAsync(DeliveryDTO dto)
    {
        SetAuth();
        var response = await _http.PostAsJsonAsync("api/entregas", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DeliveryResultDTO>();
    }

    public async Task<DeliveryResultDTO?> ResubmitDeliveryAsync(DeliveryDTO dto, int entregaAnteriorId)
    {
        SetAuth();
        var response = await _http.PostAsJsonAsync($"api/entregas/resubmit/{entregaAnteriorId}", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DeliveryResultDTO>();
    }

    public async Task<List<UsuarioResponse>> GetStudentsByCourseAsync(int cursoId)
    {
        SetAuth();
        return await _http.GetFromJsonAsync<List<UsuarioResponse>>($"api/cursos/{cursoId}/estudiantes") ?? new();
    }

    public async Task<List<DeliveryResponse>> GetDeliveriesByAssignmentAllAsync(int asignacionId)
    {
        SetAuth();
        return await _http.GetFromJsonAsync<List<DeliveryResponse>>($"api/entregas/asignacion/{asignacionId}/all") ?? new();
    }

    public async Task<string?> GetDeliveryFileContentAsync(int entregaId)
    {
        SetAuth();
        try
        {
            var result = await _http.GetFromJsonAsync<Dictionary<string, string>>($"api/entregas/{entregaId}/archivo");
            return result?.GetValueOrDefault("contenido");
        }
        catch
        {
            return null;
        }
    }

    // Retroalimentacion
    public async Task<RetroalimentacionResponse?> CreateRetroalimentacionAsync(CreateRetroalimentacionDTO dto)
    {
        SetAuth();
        var response = await _http.PostAsJsonAsync("api/retroalimentacion", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RetroalimentacionResponse>();
    }

    public async Task<List<RetroalimentacionResponse>> GetRetroalimentacionByEntregaAsync(int idEntrega)
    {
        SetAuth();
        return await _http.GetFromJsonAsync<List<RetroalimentacionResponse>>($"api/retroalimentacion/entrega/{idEntrega}") ?? new();
    }

    // Llaves
    public async Task<GenerarLlaveResponse?> GenerarLlaveCursoAsync(int cursoId)
    {
        SetAuth();
        var response = await _http.PostAsync($"api/llaves/generar/{cursoId}", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<GenerarLlaveResponse>();
    }

    public async Task<int?> RegistrarLlaveCursoAsync(RegistrarLlaveRequest request)
    {
        SetAuth();
        var response = await _http.PostAsJsonAsync("api/llaves/registrar", request);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        return result?["id"];
    }

    public async Task<LlaveCursoResponse?> ObtenerLlaveActivaAsync(int cursoId)
    {
        SetAuth();
        try
        {
            return await _http.GetFromJsonAsync<LlaveCursoResponse>($"api/llaves/activa/{cursoId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DesactivarLlaveAsync(int idLlave)
    {
        SetAuth();
        var response = await _http.PostAsync($"api/llaves/desactivar/{idLlave}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<TestResultsResponse?> ExecuteTestsAsync(int assignmentId, int entregaId, string filePath)
    {
        SetAuth();
        var response = await _http.PostAsJsonAsync("api/pruebas/ejecutar", new
        {
            assignmentId,
            entregaId,
            filePath
        });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TestResultsResponse>();
    }
}
