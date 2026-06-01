using System.Diagnostics;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Domain.Entities;

namespace PyStudentIDE.Application.Services;

public class TestEngineService : ITestEngine
{
    private readonly IUnitOfWork _unitOfWork;

    public TestEngineService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<TestResult> ExecuteTests(int assignmentId, int entregaId, string filePath)
    {
        var casos = _unitOfWork.Repository<CasoPrueba>().GetAll()
            .Where(c => c.IdAsignacion == assignmentId && c.Activo)
            .ToList();

        var results = new List<TestResult>();

        foreach (var caso in casos)
        {
            var result = ExecuteSingleTest(caso, filePath);
            results.Add(result);

            var resultadoPrueba = new ResultadoPrueba
            {
                IdEntrega = entregaId,
                IdCasoPrueba = caso.IdCasoPrueba,
                Aprobado = result.Passed,
                SalidaObtenida = result.ActualOutput,
                MensajeError = result.ErrorMessage,
                TiempoEjecucion = (decimal)result.ExecutionTimeMs,
                FechaEjecucion = DateTime.UtcNow
            };

            _unitOfWork.Repository<ResultadoPrueba>().Add(resultadoPrueba);
        }

        _unitOfWork.SaveChanges();
        return results;
    }

    private static TestResult ExecuteSingleTest(CasoPrueba caso, string filePath)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var psi = new ProcessStartInfo("python", $"\"{filePath}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            if (!string.IsNullOrEmpty(caso.Entrada))
                process.StandardInput.Write(caso.Entrada);

            process.StandardInput.Close();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(30000);
            sw.Stop();

            var actualOutput = output.TrimEnd('\r', '\n');
            var expectedOutput = caso.SalidaEsperada.TrimEnd('\r', '\n');
            var passed = string.Equals(actualOutput, expectedOutput, StringComparison.OrdinalIgnoreCase);

            return new TestResult
            {
                CaseName = caso.Nombre,
                Passed = passed,
                ExpectedOutput = caso.SalidaEsperada,
                ActualOutput = output,
                ErrorMessage = string.IsNullOrEmpty(error) ? null : error,
                ExecutionTimeMs = sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new TestResult
            {
                CaseName = caso.Nombre,
                Passed = false,
                ExpectedOutput = caso.SalidaEsperada,
                ErrorMessage = ex.Message,
                ExecutionTimeMs = sw.ElapsedMilliseconds
            };
        }
    }
}
