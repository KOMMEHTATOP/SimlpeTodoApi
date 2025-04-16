using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Data
{
    public interface ITodoContext
    {
        DbSet<ToDoItem> ToDoItems { get; }
        DbSet<UserDTO> Users { get; }
        int SaveChanges();
    }
}