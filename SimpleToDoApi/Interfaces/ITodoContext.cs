using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Interfaces
{
    public interface ITodoContext
    {
        DbSet<ToDoItem> ToDoItems { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}