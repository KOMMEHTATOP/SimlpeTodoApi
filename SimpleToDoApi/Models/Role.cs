using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.Models;

public class Role
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Название роли обязательно должно быть заполнено!")]
    public string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
}
