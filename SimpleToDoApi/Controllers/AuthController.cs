using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoApi.DTO.Auth;
using SimpleToDoApi.Interfaces.Auth;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        
        public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }
        
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                return BadRequest("Invalid username");
            }
            
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordValid)
            {
                return BadRequest("Invalid password");
            }
            
            //конечно лучше сделать так, чтобы злоумышленник не понимал существуюет ли вообще такой пользователь,
            //но пока оставил различия чтобы самому понимать что пошло не так и видеть нужные ответы
            // if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            // {
            //     return BadRequest("Invalid credentials");
            // }
            
            var token = await _tokenService.GenerateToken(user);
            return Ok(new { token = token });
        }

    }
}
