using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.User;

public class CreateUserDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "At least one role must be specified")]
    [MinLength(1, ErrorMessage = "At least one role must be specified")]
    public List<string> RoleNames { get; set; } 
}
