using System.Globalization;
using System.IO;

namespace PyStudentIDE.UI.Decorators;

public class LocalSignatureStore : ISignatureStore
{
    private readonly string _filePath;
    private const char Separator = '|';

    public LocalSignatureStore(string? filePath = null)
    {
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PyStudentIDE");
        Directory.CreateDirectory(basePath);
        _filePath = filePath ?? Path.Combine(basePath, "signatures.csv");
    }

    public ScriptSignature? GetByPath(string scriptPath)
    {
        var signatures = LoadAll();
        return signatures.FirstOrDefault(x =>
            string.Equals(x.ScriptPath, scriptPath, StringComparison.OrdinalIgnoreCase));
    }

    public void Save(ScriptSignature signature)
    {
        var signatures = LoadAll();
        var index = signatures.FindIndex(x =>
            string.Equals(x.ScriptPath, signature.ScriptPath, StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
            signatures[index] = signature;
        else
            signatures.Add(signature);

        WriteAll(signatures);
    }

    private List<ScriptSignature> LoadAll()
    {
        var list = new List<ScriptSignature>();
        if (!File.Exists(_filePath))
            return list;

        foreach (var line in File.ReadAllLines(_filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(Separator);
            if (parts.Length < 3) continue;

            if (!DateTime.TryParse(parts[2], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var signedAt))
                signedAt = DateTime.UtcNow;

            list.Add(new ScriptSignature(parts[0], parts[1], signedAt));
        }

        return list;
    }

    private void WriteAll(IEnumerable<ScriptSignature> signatures)
    {
        var lines = signatures.Select(x =>
            string.Join(Separator, x.ScriptPath, x.Hash, x.SignedAt.ToString("o", CultureInfo.InvariantCulture)));
        File.WriteAllLines(_filePath, lines);
    }
}
