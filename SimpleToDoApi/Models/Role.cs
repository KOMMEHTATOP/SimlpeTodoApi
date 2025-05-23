using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.Models;

public class Role
{
    [Key]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
}
