using SimpleToDoApi.DTO.Role;

namespace SimpleToDoApi.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRoles();
    Task<RoleDto?> GetRole(int idRole);
    Task<RoleResult> CreateRole(CreateRoleDto roleDto);
    Task<RoleResult> UpdateRole(int idRole, UpdateRoleDto roleDto);
    Task<bool> DeleteRole(int idRole);
    Task<bool> DeleteAllRoles();
}
