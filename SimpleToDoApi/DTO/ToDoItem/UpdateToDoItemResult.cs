namespace SimpleToDoApi.DTO.ToDoItem;

public class UpdateToDoItemResult
{
    public ToDoItemDto? UpdatedToDoItem { get; set; }
    public UpdateTodoItemResult? Error { get; set; }
    
    public enum UpdateTodoItemResult
    {
        ItemNotFound,
        TitleExists
    }
    
}
