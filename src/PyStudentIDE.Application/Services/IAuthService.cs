using PyStudentIDE.Application.DTOs;

namespace PyStudentIDE.Application.Services;

public interface IAuthService
{
    LoginResponse Login(LoginRequest request);
    LoginResponse Register(RegisterRequest request);
    bool ValidateToken(string token);
}
