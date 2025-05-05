using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    public string UserName { get; set; } 
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
