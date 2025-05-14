using SimpleToDoApi.DTO.Role;

namespace SimpleToDoApi.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRoles();
    Task<RoleDto?> GetRole(string idRole);
    Task<RoleResult> CreateRole(CreateRoleDto roleDto);
    Task<RoleResult> UpdateRole(string idRole, UpdateRoleDto roleDto);
    Task<bool> DeleteRole(string idRole);
    Task<bool> DeleteAllRoles();
}
