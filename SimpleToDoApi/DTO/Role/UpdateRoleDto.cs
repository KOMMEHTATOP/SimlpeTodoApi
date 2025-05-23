using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.Role;

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
