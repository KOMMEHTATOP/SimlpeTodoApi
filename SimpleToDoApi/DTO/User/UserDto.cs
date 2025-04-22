using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO
{
    public class UserDto
    {
        [Required(ErrorMessage = "Название роли должно быть обязательно заполнено!")]
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
