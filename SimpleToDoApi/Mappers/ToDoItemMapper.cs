using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Mappers;

public static class ToDoItemMapper
{
    public static ToDoItemDto ToDto(ToDoItem model)
    {
        return new ToDoItemDto { Title = model.Title, Description = model.Description, IsComplete = model.IsComplete };
    }

    public static ToDoItem FromDto(CreateToDoItemDto dto)
    {
        return new ToDoItem { Title = dto.Title, Description = dto.Description, IsComplete = dto.IsComplete, Updated = DateTime.Now, CreatedByUserId = 1};
    }
}
