namespace SimpleToDoApi.DTO.User;

public class UserQueryParameters
{
    private const int MaxPageSize = 50; // Максимальный размер страницы
    private int _pageSize = 10; // Размер страницы по умолчанию

    public int PageNumber { get; set; } = 1; // Номер страницы по умолчанию
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? EmailContains { get; set; }
    public string? UserNameContains { get; set; }
    public string? RoleName { get; set; }

}
