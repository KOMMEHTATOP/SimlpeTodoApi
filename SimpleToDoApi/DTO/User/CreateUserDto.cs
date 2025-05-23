namespace SimpleToDoApi.DTO.User;

public class CreateUserDto
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public List<string> RoleIds { get; set; } = new();

}
