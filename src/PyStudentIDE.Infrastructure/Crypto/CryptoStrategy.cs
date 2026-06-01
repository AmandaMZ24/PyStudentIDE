using System.Security.Cryptography;
using PyStudentIDE.Application.Services;

namespace PyStudentIDE.Infrastructure.Crypto;

public interface ICryptoStrategy
{
    string ComputeHash(byte[] content);
    bool VerifyHash(byte[] content, string hash);
}

public class Sha256Strategy : ICryptoStrategy
{
    public string ComputeHash(byte[] content)
    {
        var hashBytes = SHA256.HashData(content);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public bool VerifyHash(byte[] content, string hash)
    {
        var computed = ComputeHash(content);
        return string.Equals(computed, hash, StringComparison.OrdinalIgnoreCase);
    }
}

public class HashService : IHashService
{
    private readonly ICryptoStrategy _strategy;

    public HashService(ICryptoStrategy strategy)
    {
        _strategy = strategy;
    }

    public string ComputeSHA256(byte[] content)
    {
        return _strategy.ComputeHash(content);
    }

    public bool VerifyHash(byte[] content, string hash)
    {
        return _strategy.VerifyHash(content, hash);
    }
}
