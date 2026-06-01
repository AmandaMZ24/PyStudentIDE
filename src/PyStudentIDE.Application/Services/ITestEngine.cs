namespace PyStudentIDE.Application.Services;

public class TestResult
{
    public string CaseName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? ExpectedOutput { get; set; }
    public string? ActualOutput { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal ExecutionTimeMs { get; set; }
}

public interface ITestEngine
{
    IEnumerable<TestResult> ExecuteTests(int assignmentId, int entregaId, string filePath);
}
