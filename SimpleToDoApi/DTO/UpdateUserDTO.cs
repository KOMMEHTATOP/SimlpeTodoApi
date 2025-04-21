namespace SimpleToDoApi.DTO;

public class UpdateUserDTO
{
    public string? UserName { get; set; }
    public List<int> RoleIds { get; set; } = new();
}
