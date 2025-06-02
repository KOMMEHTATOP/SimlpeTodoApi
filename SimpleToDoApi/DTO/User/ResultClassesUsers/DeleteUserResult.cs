using Microsoft.AspNetCore.Identity;

namespace SimpleToDoApi.DTO.User.HelpersClassToService
{
    public class DeleteUserResult
    {
        public bool Succeeded { get; private set; }
        public List<string> Errors { get; private set; } = new List<string>();

        // Приватный конструктор, чтобы создание шло через фабричные методы
        private DeleteUserResult(bool succeeded, IEnumerable<string>? errors = null)
        {
            Succeeded = succeeded;
            if (errors != null)
            {
                Errors.AddRange(errors);
            }
        }

        public static DeleteUserResult Success()
        {
            return new DeleteUserResult(true);
        }

        public static DeleteUserResult Failed(IEnumerable<IdentityError> identityErrors)
        {
            return new DeleteUserResult(false, identityErrors.Select(e => e.Description));
        }

        public static DeleteUserResult Failed(string errorMessage)
        {
            return new DeleteUserResult(false, new List<string> { errorMessage });
        }

        // Специфичная ошибка для "не найден"
        public static DeleteUserResult UserNotFound()
        {
            return Failed("User not found.");
        }
    }
}
