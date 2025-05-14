using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.Auth;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    [Required, MinLength(8)]
    public string Password { get; set; } = null!;
    [Required, Compare("Password")]
    public string ConfirmPassword { get; set; } = null!;
}
