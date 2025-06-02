using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Mappers;

public static class RoleMapper
{
    public static RoleDto ToDto(ApplicationRole role)  
    {
        return new RoleDto()
        {
            Id = role.Id, 
            Name = role.Name, 
            Description = role.Description
        };
    }

    public static ApplicationRole FromDto(CreateRoleDto roleDto) 
    {
        return new ApplicationRole(roleDto.Name!)
        {
            Description = roleDto.Description
        };
    }
}

