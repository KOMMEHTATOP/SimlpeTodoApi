namespace SimpleToDoApi.DTO.User;

public class UpdateUserDto
{
    public required string UserName { get; set; }
    public string Email { get; set; }
    public List<string> RoleIds { get; set; } = new();
}
