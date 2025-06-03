using SimpleToDoApi.Models;

namespace SimpleToDoApi.Interfaces.Auth;

public interface ITokenService
{
    public Task<string> GenerateToken(ApplicationUser user);
}
