using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.Role;

public class CreateRoleDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Название роли обязательно")]
    public string Name { get; set; }
    public string Description { get; set; }
}
