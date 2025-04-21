using SimpleToDoApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
