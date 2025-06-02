using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleToDoApi.Models;

namespace SimpleToDoApi.Interfaces
{
    public interface ITodoContext
    {
        DbSet<ToDoItem> ToDoItems { get; }
        DbSet<IdentityUserRole<string>> UserRoles { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}