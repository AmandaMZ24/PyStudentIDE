using System;
using System.Security.Cryptography;
using System.Text;

namespace DecoratorApp.Decorators;

public class SignedScriptDecorator : ScriptDecorator
{
    private readonly ISignatureStore _store;

    public SignedScriptDecorator(IScript inner, ISignatureStore store) : base(inner)
    {
        _store = store;
    }

    public string GenerateSignature()
    {
        var hash = ComputeHash(GetText());
        _store.Save(new ScriptSignature(GetPath(), hash, DateTime.UtcNow));
        return hash;
    }

    public bool VerifySignature()
    {
        var stored = _store.GetByPath(GetPath());
        if (stored == null) return false;
        var current = ComputeHash(GetText());
        return string.Equals(stored.Hash, current, StringComparison.OrdinalIgnoreCase);
    }

    public string RegenerateSignature() => GenerateSignature();

    private static string ComputeHash(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
