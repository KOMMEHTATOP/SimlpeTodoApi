using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SimpleToDoApi.Interfaces.Auth;
using SimpleToDoApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleToDoApi.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }
    
    public async Task<string> GenerateToken(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id), 
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
        };

        foreach (var role in userRoles)
        {
            userClaims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        //По сути тоже самое что перебор выше
        //userClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        //создаем подпись
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key не найден в конфигурации");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        //параметры токена
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],           // Кто выдал токен
            audience: _configuration["Jwt:Audience"],       // Для кого токен
            claims: userClaims,                             // Твои claims
            //expires: DateTime.UtcNow.AddMinutes(60),      // Хардкод времени жизни, другой вариант из настроек ниже 
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"]!)),// Когда истекает
            signingCredentials: credentials                 // Подпись токена
        );
        
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}
