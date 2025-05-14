using SimpleToDoApi.Data;
using SimpleToDoApi.DTO.ToDoItem;
using SimpleToDoApi.Mappers;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Interfaces;

namespace SimpleToDoApi.Services;

public class ToDoService(ITodoContext dbContext, IDatabaseCleaner databaseCleaner) : IToDoService
{

    public async Task<GetToDoItemsResult> GetAllToDo(ToDoItemFilterDto filter)
    {
        var query = dbContext.ToDoItems.AsQueryable();

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
            var userExists = await dbContext.Users.AnyAsync(user => user.Id == filter.UserId);

            if (!userExists)
            {
                return new GetToDoItemsResult()
                {
                    Error = GetToDoItemsResult.GetToDoItemsError.UserNotFound
                };
            }

            query = query.Where(i => i.CreatedByUserId == filter.UserId);
        }

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

    public async Task<ToDoItemResult> GetByIdToDo(int id)
    {
        var todoItem = await dbContext.ToDoItems.FindAsync(id);

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

    public async Task<CreateToDoResult> CreateToDo(CreateToDoItemDto createToDoItemDto)
    {
        bool titleExists = await dbContext.ToDoItems.AnyAsync(item => item.Title == createToDoItemDto.Title);
        if (titleExists)
        {
            return new CreateToDoResult
            {
                Error = CreateToDoResult.CreateToDoItemDtoError.TitleExists
            };
        }

        var newToDoItem = ToDoItemMapper.FromDto(createToDoItemDto);
        dbContext.ToDoItems.Add(newToDoItem);
        await dbContext.SaveChangesAsync();
        
        return new CreateToDoResult()
        {
            CreatedItem = ToDoItemMapper.ToDto(newToDoItem),
        };
    }
    
    public async Task<UpdateToDoItemResult> UpdateToDo(int id, UpdateToDoItemDto updateToDoItemDto)
    {
       var todoItem = await dbContext.ToDoItems.FindAsync(id);

       if (todoItem == null)
       {
           return new UpdateToDoItemResult
           {
               Error = UpdateToDoItemResult.UpdateTodoItemResult.ItemNotFound
           };
       }
       
       bool titleExists = await dbContext.ToDoItems
            .AnyAsync(t => t.Title == updateToDoItemDto.Title && t.Id != id);

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

        await dbContext.SaveChangesAsync();
        
        return new UpdateToDoItemResult()
        {
            UpdatedToDoItem = ToDoItemMapper.ToDto(todoItem)
        };
    }

    public async Task<bool> DeleteId(int id)
    {
        var todoItem = await dbContext.ToDoItems.FindAsync(id);

        if (todoItem == null)
        {
            return false;
        }

        dbContext.ToDoItems.Remove(todoItem);
        await dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteAll()
    {
        var any = await dbContext.ToDoItems.AnyAsync();
        await databaseCleaner.ClearTodoItems();
        return any;
    }
}
