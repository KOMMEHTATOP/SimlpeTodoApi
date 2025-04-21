using System.ComponentModel.DataAnnotations;
// ReSharper disable All

namespace SimpleToDoApi.DTO;

public class UpdateUserDto
{
    [Required]
    public required string UserName { get; set; }
    [Required]
    public List<int> RoleIds { get; set; } = new();
}
