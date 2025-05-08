namespace SimpleToDoApi.DTO.ToDoItem;

public class CreateToDoResult
{
    public ToDoItemDto? CreatedItem { get; set; }
    public CreateToDoItemDtoError? Error { get; set; }

    public enum CreateToDoItemDtoError
    {
        TitleExists
    }
}
