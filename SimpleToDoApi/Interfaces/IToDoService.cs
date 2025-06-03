using SimpleToDoApi.DTO.ToDoItem;

namespace SimpleToDoApi.Interfaces;

public interface IToDoService
{
    Task<GetToDoItemsResult> GetAllToDo(ToDoItemFilterDto filter, string userId);
    Task<ToDoItemResult> GetByIdToDo(int id, string userId);
    Task<CreateToDoResult> CreateToDo(CreateToDoItemDto createToDoItemDto, string userId);
    Task<UpdateToDoItemResult> UpdateToDo(int id, UpdateToDoItemDto updateToDoItemDto, string userId);
    Task<bool> DeleteId(int id, string userId);
}
