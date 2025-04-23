using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Mappers;

public static class RoleMapper
{
    public static RoleDto ToDto(this Role role)
    {
        return new RoleDto()
        {
            Name = role.Name, Description = role.Description
        };
    }

    public static Role FromDto(CreateRoleDto roleDto)
    {
        return new Role()
        {
            Name = roleDto.Name, Description = roleDto.Description
        };
    }
}

