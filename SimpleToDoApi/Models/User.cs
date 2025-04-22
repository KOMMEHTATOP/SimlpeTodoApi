using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } 
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
