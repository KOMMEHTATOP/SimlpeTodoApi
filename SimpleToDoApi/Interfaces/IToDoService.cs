using SimpleToDoApi.DTO;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Interfaces;

public interface IToDoService
{
    Task<PagedResult<ToDoItemDto>> GetAllAsync(ToDoItemFilterDto filter);
    Task<ToDoItemDto?> GetByIdAsync(int id);
    Task<ToDoItemDto?> CreateAsync(CreateToDoItemDto createToDoItemDto);
    Task<ToDoItemDto?> UpdateAsync(int id, UpdateToDoItemDto updateToDoItemDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllAsync();
}
