namespace SimpleToDoApi.DTO.Role;

public class RoleResult
{
    public RoleDto? Role { get; set; }
    public RoleError? Error { get; set; }

    public enum RoleError
    {
        RoleExists,
        RoleNotFound
    }
}
