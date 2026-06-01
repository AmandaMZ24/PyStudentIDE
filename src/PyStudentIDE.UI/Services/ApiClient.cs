using System.Net.Http;
using System.Net.Http.Json;
using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.UI.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http) { _http = http; }

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
        return await _http.GetFromJsonAsync<List<CursoResponse>>($"api/cursos/usuario/{usuarioId}") ?? new();
    }

    public async Task<CursoResponse?> CreateCourseAsync(CreateCursoRequest request, int docenteId)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, "api/cursos");
        msg.Headers.Add("docenteId", docenteId.ToString());
        msg.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(msg);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CursoResponse>();
    }

    public async Task<bool> JoinCourseAsync(int cursoId, JoinCursoRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/cursos/{cursoId}/join", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AssignmentDTO>> GetAssignmentsByCourseAsync(int cursoId)
    {
        return await _http.GetFromJsonAsync<List<AssignmentDTO>>($"api/asignaciones/curso/{cursoId}") ?? new();
    }

    public async Task<int?> CreateAssignmentAsync(AssignmentDTO dto, int docenteId)
    {
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
        var response = await _http.PutAsJsonAsync($"api/asignaciones/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<DeliveryResultDTO?> SubmitDeliveryAsync(DeliveryDTO dto)
    {
        var response = await _http.PostAsJsonAsync("api/entregas", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DeliveryResultDTO>();
    }
}
