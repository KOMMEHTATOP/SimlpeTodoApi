using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.User;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Mappers;

public static class UserMapper
{
    public static ApplicationUser ToApplicationUser(CreateUserDto dto)
    {
        return new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email
        };
    }
    
    public static UserDto ToDto(ApplicationUser user, IList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Roles = roles?.ToList() ?? new List<string>()
        };
    }


}