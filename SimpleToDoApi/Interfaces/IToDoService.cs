using SimpleToDoApi.DTO.ToDoItem;

namespace SimpleToDoApi.Interfaces;

public interface IToDoService
{
    Task<GetToDoItemsResult> GetAllToDo(ToDoItemFilterDto filter);
    Task<ToDoItemResult> GetByIdToDo(string id);
    Task<CreateToDoResult> CreateToDo(CreateToDoItemDto createToDoItemDto);
    Task<UpdateToDoItemResult> UpdateToDo(string id, UpdateToDoItemDto updateToDoItemDto);
    Task<bool> DeleteId(string id);
    Task<bool> DeleteAll();
}
