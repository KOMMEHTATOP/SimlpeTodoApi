using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO;

public class CreateUserDTO
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    public List<int> RoleIds { get; set; } = new();

}
