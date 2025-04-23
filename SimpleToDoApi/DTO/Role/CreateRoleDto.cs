using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.Role;

public class CreateRoleDto
{
    [Required(ErrorMessage = "Название роли обязательно")]
    public string Name { get; set; }
    public string Description { get; set; }
}
