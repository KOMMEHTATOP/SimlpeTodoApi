namespace SimpleToDoApi.DTO;

public class CreateUserResult
{
    public UserDto? User { get; set; }
    public CreateUserError? Error { get; set; }

    public enum CreateUserError
    {
        UserNameExists,
        EmailExists,
        RoleNotFound
    }
}
