using SimpleToDoApi.DTO.Role;
using SimpleToDoApi.DTO.User;

namespace SimpleToDoApi.DTO;

public class UpdateUserResult
{
    public UserDto? User { get; set; }
    public UpdateUserError? Error { get; set; }

    public enum UpdateUserError
    {
        UserNotFound,
        RoleNotFound,
        UserNameExists,
        UserEmailExists,
        NoRolesProvided 
    }
}
