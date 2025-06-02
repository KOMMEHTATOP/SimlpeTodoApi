namespace SimpleToDoApi.DTO.Role.ResultClassesRole;

public class RoleResult
{
    public RoleDto? Role { get; set; } 
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static RoleResult Success()
    {
        return new RoleResult() {Succeeded = true};
    }
    public static RoleResult Success(RoleDto role)
    {
        return new RoleResult {Succeeded = true, Role = role};
    }
    
    public static RoleResult Failed(string error)
    {
        return new RoleResult {Succeeded = false, Errors = new List<string>{error}};
    }

    public static RoleResult Failed(List<string> errors)
    {
        return new RoleResult {Succeeded = false, Errors = errors};
    }

}
