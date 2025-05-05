using System.ComponentModel.DataAnnotations;
// ReSharper disable All

namespace SimpleToDoApi.DTO;

public class UpdateUserDto
{
    [Required]
    public required string UserName { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public List<int> RoleIds { get; set; } = new();
}
