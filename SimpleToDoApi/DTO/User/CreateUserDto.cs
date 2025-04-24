using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO;

public class CreateUserDto
{
    [Required]
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<int> RoleIds { get; set; } = new();

}
