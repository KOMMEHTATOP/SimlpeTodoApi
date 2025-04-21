using SimpleToDoApi.DTO;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Mappers;

public static class UserMapper
{
    public static UserDTO ToDTO(User user) => new UserDTO 
        { 
            Id = user.Id,
            UserName = user.UserName, 
            Email = user.Email, 
            Roles = user.Roles.Select(r=>r.Name).ToList() ?? new List<string>()
        };

    public static User FromDTO(CreateUserDTO dto, IEnumerable<Role> allRoles, string passwordHash)
    {
        return new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Roles = allRoles.ToList()
        };
    }
}
