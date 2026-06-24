using System.Text.Json.Serialization;

namespace PyStudentIDE.Application.DTOs;

public class TestResultDTO
{
    [JsonPropertyName("caseName")]
    public string CaseName { get; set; } = string.Empty;

    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    [JsonPropertyName("expectedOutput")]
    public string? ExpectedOutput { get; set; }

    [JsonPropertyName("actualOutput")]
    public string? ActualOutput { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("executionTimeMs")]
    public decimal ExecutionTimeMs { get; set; }
}

public class TestResultsResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("aprobadas")]
    public int Aprobadas { get; set; }

    [JsonPropertyName("fallidas")]
    public int Fallidas { get; set; }

    [JsonPropertyName("resultados")]
    public List<TestResultDTO> Resultados { get; set; } = new();
}
