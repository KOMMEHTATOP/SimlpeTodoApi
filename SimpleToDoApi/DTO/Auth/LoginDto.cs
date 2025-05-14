using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.Auth;

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}
