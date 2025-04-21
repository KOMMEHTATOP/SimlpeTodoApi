using SimpleToDoApi.DTO;
using SimpleToDoApi.Models;
using SimpleToDoApi.Models.Enums;

namespace SimpleToDoApi.Mappers;

public static class UserMapper
{
    public static UserDTO ToDTO(User user) => new UserDTO 
        { 
            UserName = user.UserName, 
            Email = user.Email, 
            Role = user.Role.ToString() 
        };

    public static User FromDTO(UserDTO dto)
    {
        Enum.TryParse(dto.Role, out UserRole role);
        return new User
        {
            Id = dto.Id,
            UserName = dto.UserName, 
            Email = dto.Email, 
            Role = role
        };
    }
}
