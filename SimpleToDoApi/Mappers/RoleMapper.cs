using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Mappers;

public static class RoleMapper
{
    public static RoleDto ToDto(Role role)
    {
        return new RoleDto()
        {
            Id = role.Id, Name = role.Name
        };
    }

    public static Role FromDto(CreateRoleDto roleDto)
    {
        return new Role()
        {
            Name = roleDto.Name
        };
    }
}

