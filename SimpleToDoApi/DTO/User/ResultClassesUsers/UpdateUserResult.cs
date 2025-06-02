using Microsoft.AspNetCore.Identity;

namespace SimpleToDoApi.DTO.User.ResultClassesUsers 
{
    public class UpdateUserResult
    {
        public UserDto? User { get; private set; } 
        public List<string> Errors { get; private set; } = new List<string>();

        public bool Succeeded
        {
            get => User != null && Errors.Count == 0;
        }

        public UpdateUserResult(UserDto user)
        {
            User = user;
        }

        private UpdateUserResult(IEnumerable<string>? errors) // Сделаем его приватным, чтобы создание шло через Failed
        {
            if (errors != null)
            {
                Errors.AddRange(errors);
            }
        }

        public static UpdateUserResult Failed(IEnumerable<IdentityError> identityErrors)
        {
            return new UpdateUserResult(identityErrors.Select(e => e.Description));
        }

        public static UpdateUserResult Failed(string errorMessage)
        {
            return new UpdateUserResult(new List<string> { errorMessage });
        }
    }
}