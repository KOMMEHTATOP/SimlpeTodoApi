using SimpleToDoApi.DTO.ToDoItem;

namespace SimpleToDoApi.Interfaces;

public interface IToDoService
{
    Task<GetToDoItemsResult> GetAllToDo(ToDoItemFilterDto filter);
    Task<ToDoItemResult> GetByIdToDo(int id);
    Task<CreateToDoResult> CreateToDo(CreateToDoItemDto createToDoItemDto);
    Task<UpdateToDoItemResult> UpdateToDo(int id, UpdateToDoItemDto updateToDoItemDto);
    Task<bool> DeleteId(int id);
    Task<bool> DeleteAll();
}
