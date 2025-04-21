using SimpleToDoApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.Models;

public class User
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
}
