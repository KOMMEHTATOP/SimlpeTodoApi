namespace SimpleToDoApi.DTO.ToDoItem;

public class ToDoItemResult
{
    public ToDoItemDto? Item { get; set; }
    public ToDoItemError? Error { get; set; }
    
    public enum ToDoItemError
    {
        ToDoNotFound,
    }
}
