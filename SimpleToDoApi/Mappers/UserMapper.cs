using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.User;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(User user, IList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = roles.ToList()
        };
    }

    public static User FromDto(CreateUserDto dto)
    {
        return new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PasswordHash = null, // Сервис заполнит
            Roles = new List<Role>() // Сервис добавит
        };
    }
}
