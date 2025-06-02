namespace SimpleToDoApi.DTO.User.ResultClassesUsers;

public class UserQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    // Фильтры:
    public string? EmailContains { get; set; }
    public string? UserNameContains { get; set; }
    public string? RoleName { get; set; }
}
