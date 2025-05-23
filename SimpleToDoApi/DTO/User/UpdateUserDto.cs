namespace SimpleToDoApi.DTO.User;

public class UpdateUserDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public List<string> RoleNames { get; set; } = new();
}
