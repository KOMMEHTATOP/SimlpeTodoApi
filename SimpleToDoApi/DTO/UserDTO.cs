using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO
{
    public class UserDTO
    {
        public int id { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Role { get; set; }
    }
}
