using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO;

public class CreateUserDto
{
    [Required]
    public string UserName { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }
    public List<string> RoleIds { get; set; } = new();

}
