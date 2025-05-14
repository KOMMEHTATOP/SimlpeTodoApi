using SimpleToDoApi.DTO.Auth;

namespace SimpleToDoApi.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<AuthResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
