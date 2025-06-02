using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.DTO.Role.ResultClassesRole;

namespace SimpleToDoApi.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleAsync(string idRole);
    Task<RoleResult> CreateRoleAsync(CreateRoleDto roleDto);
    Task<RoleResult> UpdateRoleAsync(string idRole, UpdateRoleDto roleDto);
    Task<RoleResult> DeleteRoleAsync(string idRole);
}
