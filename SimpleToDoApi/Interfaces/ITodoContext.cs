using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.DTO;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Data
{
    public interface ITodoContext
    {
        DbSet<ToDoItem> ToDoItems { get; }
        DbSet<User> Users { get; }
        int SaveChanges();
    }
}