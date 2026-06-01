using System.Security.Cryptography;
using System.Text;

namespace PyStudentIDE.UI.Services;

public class ScriptSignatureHeaderService
{
    public const string SignaturePrefix = "# SIGNATURE:";

    public string AddOrReplaceSignature(string body)
    {
        var normalizedBody = body.Replace("\r\n", "\n").TrimEnd();
        var hash = ComputeHash(normalizedBody);
        return $"{SignaturePrefix}{hash}\n{normalizedBody}\n";
    }

    public bool HasSignature(string content)
    {
        var firstLine = GetFirstLine(content);
        return firstLine.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase);
    }

    public bool VerifySignature(string content, out string? expectedHash)
    {
        expectedHash = null;
        var (signature, body) = ExtractSignature(content);
        if (signature == null) return false;
        expectedHash = signature;
        var computed = ComputeHash(body);
        return string.Equals(signature, computed, StringComparison.OrdinalIgnoreCase);
    }

    public string StripSignature(string content)
    {
        var (_, body) = ExtractSignature(content);
        return body;
    }

    public string ComputeHash(string body)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static (string? signature, string body) ExtractSignature(string content)
    {
        if (string.IsNullOrEmpty(content)) return (null, string.Empty);
        var lines = content.Replace("\r\n", "\n").Split('\n');
        if (lines.Length == 0) return (null, string.Empty);
        var first = lines[0].Trim();
        if (!first.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return (null, content.Replace("\r\n", "\n").TrimEnd());
        }

        var signature = first.Substring(SignaturePrefix.Length).Trim();
        var body = string.Join("\n", lines.Skip(1)).TrimEnd();
        return (signature, body);
    }

    private static string GetFirstLine(string content)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;
        var index = content.IndexOf('\n');
        return index < 0 ? content.Trim() : content.Substring(0, index).Trim();
    }
}
