namespace SimpleToDoApi.DTO.Auth;

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; } // Для JWT
    public List<string>? Errors { get; set; }

    // Удобные методы для создания результата
    public static AuthResult Success(string? token = null) 
        => new() { IsSuccess = true, Token = token };
    
    public static AuthResult Failure(List<string> errors) 
        => new() { IsSuccess = false, Errors = errors };
    
    public static AuthResult Failure(string singleError) 
        => new() { IsSuccess = false, Errors = new List<string> { singleError } };
}
