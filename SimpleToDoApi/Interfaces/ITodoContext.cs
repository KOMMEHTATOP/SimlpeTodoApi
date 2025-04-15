using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Data
{
    public interface ITodoContext
    {
        DbSet<TodoItem> ToDoItems { get; }
        DbSet<UserDTO> Users { get; }
        int SaveChanges();
    }
}