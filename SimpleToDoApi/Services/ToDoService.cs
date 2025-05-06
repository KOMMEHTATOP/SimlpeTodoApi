using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Mappers;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Interfaces;

namespace SimpleToDoApi.Services;

public class ToDoService : IToDoService
{
    private readonly ITodoContext _context;
    private readonly IDatabaseCleaner _databaseCleaner;
    public ToDoService(ITodoContext dbContext, IDatabaseCleaner databaseCleaner)
    {
        _context = dbContext;
        _databaseCleaner = databaseCleaner;
    }

    public async Task<PagedResult<ToDoItemDto>> GetAllAsync(ToDoItemFilterDto filter)
    {
        var query = _context.ToDoItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(item => item.Title.Contains(filter.Search) || item.Description.Contains(filter.Search));
        }

        if (filter.IsComplete.HasValue)
        {
            query = query.Where(item => item.IsComplete == filter.IsComplete);
        }

        if (filter.UserId > 0)
        {
            var userExists = await _context.Users.AnyAsync(user => user.Id == filter.UserId);

            if (!userExists)
            {
                throw new Exception("User does not exist.");
            }

            query = query.Where(i => i.CreatedByUserId == filter.UserId);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(ToDoItemMapper.ToDto).ToList();

        return new PagedResult<ToDoItemDto>
        {
            TotalCount = totalCount, Page = filter.Page, PageSize = filter.PageSize, Data = dtos
        };
    }

    public async Task<ToDoItemDto?> GetByIdAsync(int id)
    {
        var todoItem = await _context.ToDoItems.FindAsync(id);

        if (todoItem == null)
        {
            return null;
        }

        return ToDoItemMapper.ToDto(todoItem);
    }

    public async Task<ToDoItemDto?> CreateAsync(CreateToDoItemDto createToDoItemDto)
    {
        if (await _context.ToDoItems.AnyAsync(t => t.Title == createToDoItemDto.Title))
        {
            return null;
        }

        var newToDoItem = ToDoItemMapper.FromDto(createToDoItemDto);
        _context.ToDoItems.Add(newToDoItem);
        await _context.SaveChangesAsync();
        return ToDoItemMapper.ToDto(newToDoItem);
    }
    
    public async Task<ToDoItemDto> UpdateAsync(int id, UpdateToDoItemDto updateToDoItemDto)
    {
        // Проверка на уникальность названия (кроме текущей)
        bool titleExists = await _context.ToDoItems
            .AnyAsync(t => t.Title == updateToDoItemDto.Title && t.Id != id);

        if (titleExists)
            return null; // Можно выбросить исключение или вернуть ошибку, если хочешь

        var entity = await _context.ToDoItems.FindAsync(id);

        if (entity == null)
            return null;

        entity.Title = updateToDoItemDto.Title;
        entity.Description = updateToDoItemDto.Description;
        entity.IsComplete = updateToDoItemDto.IsComplete;
        entity.Updated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDoItemMapper.ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todoItem = await _context.ToDoItems.FindAsync(id);

        if (todoItem == null)
        {
            return false;
        }

        _context.ToDoItems.Remove(todoItem);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeleteAllAsync()
    {
        // Можно проверить, были ли вообще задачи
        var any = await _context.ToDoItems.AnyAsync();
        await _databaseCleaner.ClearTodoItems();
        return any; // true если что-то было удалено, false если таблица и так была пуста
    }
}
