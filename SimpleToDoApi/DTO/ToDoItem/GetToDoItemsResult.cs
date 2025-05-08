namespace SimpleToDoApi.DTO.ToDoItem;

public class GetToDoItemsResult
{
    public ToDoMetaPage<ToDoItemDto>? Page { get; set; }
    public GetToDoItemsError? Error { get; set; }
    
    public enum GetToDoItemsError
    {
        UserNotFound
    }

}
