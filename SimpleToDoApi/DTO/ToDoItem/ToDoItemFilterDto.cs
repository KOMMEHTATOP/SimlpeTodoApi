namespace SimpleToDoApi.DTO;

public class ToDoItemFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? IsComplete { get; set; }
    public string? Search { get; set; }
    public int UserId { get; set; }
}
