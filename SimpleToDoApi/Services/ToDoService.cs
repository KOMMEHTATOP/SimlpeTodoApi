using Microsoft.AspNetCore.Identity;
using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Mappers;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Interfaces;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Services;

public class ToDoService : IToDoService
{
    private readonly ITodoContext _context;
    private readonly IDatabaseCleaner _databaseCleaner;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public ToDoService(ITodoContext dbContext, IDatabaseCleaner databaseCleaner, UserManager<ApplicationUser> userManager)
    {
        _context = dbContext;
        _databaseCleaner = databaseCleaner;
        _userManager = userManager;
    }

    public async Task<GetToDoItemsResult> GetAllToDo(ToDoItemFilterDto filter, string userId)
    {
        // Показываем только задачи текущего пользователя
        var query = _context.ToDoItems.Where(item => item.CreatedByUserId == userId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(item => item.Title.Contains(filter.Search) || item.Description.Contains(filter.Search));
        }

        if (filter.IsComplete.HasValue)
        {
            query = query.Where(item => item.IsComplete == filter.IsComplete);
        }
        
        // Убираем фильтр по UserName - теперь показываем только задачи текущего пользователя
        // if (!string.IsNullOrWhiteSpace(filter.UserName))
        // {
        //     var user = await _userManager.FindByNameAsync(filter.UserName);
        //     if (user != null)
        //     {
        //         query = query.Where(item => item.CreatedByUserId == user.Id);
        //     }
        // }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(ToDoItemMapper.ToDto).ToList();

        return new GetToDoItemsResult()
        {
            Page = new ToDoMetaPage<ToDoItemDto>()
            {
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Data = dtos
            }
        };
    }

    public async Task<ToDoItemResult> GetByIdToDo(int id, string userId)
    {
        // Проверяем что задача принадлежит пользователю
        var todoItem = await _context.ToDoItems.FirstOrDefaultAsync(item => item.Id == id && item.CreatedByUserId == userId);

        if (todoItem == null)
        {
            return new ToDoItemResult
            {
                Error = ToDoItemResult.ToDoItemError.ToDoNotFound
            };
        }
        
        var dto = ToDoItemMapper.ToDto(todoItem);
        return new ToDoItemResult()
        {
            Item = dto
        };
    }

    public async Task<CreateToDoResult> CreateToDo(CreateToDoItemDto createToDoItemDto, string userId)
    {
        // Проверяем уникальность title только среди задач этого пользователя
        bool titleExists = await _context.ToDoItems.AnyAsync(item => item.Title == createToDoItemDto.Title && item.CreatedByUserId == userId);
        if (titleExists)
        {
            return new CreateToDoResult
            {
                Error = CreateToDoResult.CreateToDoItemDtoError.TitleExists
            };
        }

        var newToDoItem = ToDoItemMapper.FromDto(createToDoItemDto);
        newToDoItem.CreatedByUserId = userId; // Автоматически ставим владельца
        
        _context.ToDoItems.Add(newToDoItem);
        await _context.SaveChangesAsync();
        
        return new CreateToDoResult()
        {
            CreatedItem = ToDoItemMapper.ToDto(newToDoItem),
        };
    }
    
    public async Task<UpdateToDoItemResult> UpdateToDo(int id, UpdateToDoItemDto updateToDoItemDto, string userId)
    {
        // Проверяем что задача принадлежит пользователю
        var todoItem = await _context.ToDoItems.FirstOrDefaultAsync(item => item.Id == id && item.CreatedByUserId == userId);

        if (todoItem == null)
        {
            return new UpdateToDoItemResult
            {
                Error = UpdateToDoItemResult.UpdateTodoItemResult.ItemNotFound
            };
        }
       
        // Проверяем уникальность title только среди задач этого пользователя
        bool titleExists = await _context.ToDoItems
             .AnyAsync(t => t.Title == updateToDoItemDto.Title && t.Id != id && t.CreatedByUserId == userId);

        if (titleExists)
        {
            return new UpdateToDoItemResult()
            {
                Error = UpdateToDoItemResult.UpdateTodoItemResult.TitleExists
            };
        }

        todoItem.Title = updateToDoItemDto.Title;
        todoItem.Description = updateToDoItemDto.Description;
        todoItem.IsComplete = updateToDoItemDto.IsComplete;
        todoItem.Updated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        return new UpdateToDoItemResult()
        {
            UpdatedToDoItem = ToDoItemMapper.ToDto(todoItem)
        };
    }

    public async Task<bool> DeleteId(int id, string userId)
    {
        // Проверяем что задача принадлежит пользователю
        var todoItem = await _context.ToDoItems.FirstOrDefaultAsync(item => item.Id == id && item.CreatedByUserId == userId);

        if (todoItem == null)
        {
            return false;
        }

        _context.ToDoItems.Remove(todoItem);
        await _context.SaveChangesAsync();
        return true;
    }
}