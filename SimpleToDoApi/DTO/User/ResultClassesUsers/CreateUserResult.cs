using Microsoft.AspNetCore.Identity;

namespace SimpleToDoApi.DTO.User.HelpersClassToService;

public class CreateUserResult
{
    public UserDto? User { get; private set; } 
    public List<string> Errors { get; private set; } = new List<string>();

    public bool Succeeded
    {
        get => User != null && Errors.Count == 0;
    }

    public CreateUserResult(UserDto user)
    {
        User = user;
    }

    public CreateUserResult(IEnumerable<string> errors)
    {
        Errors.AddRange(errors);
    }

    // Статический метод для удобного создания результата с ошибками из IdentityResult
    public static CreateUserResult Failed(IEnumerable<IdentityError> identityErrors) 
    {
        return new CreateUserResult(identityErrors.Select(e => e.Description));
    }

    // Статический метод для удобного создания результата с одной ошибкой (если нужно)
    public static CreateUserResult Failed(string errorMessage)
    {
        return new CreateUserResult(new List<string> { errorMessage });
    }
}

