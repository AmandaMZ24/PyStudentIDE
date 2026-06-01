namespace PyStudentIDE.Application.Services;

public interface IHashService
{
    string ComputeSHA256(byte[] content);
    bool VerifyHash(byte[] content, string hash);
}
