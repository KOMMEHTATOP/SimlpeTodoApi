namespace SimpleToDoApi.Data;

public interface IDatabaseCleaner
{
    Task ClearTodoItems();
    Task ClearUsers();
    Task ClearRole();
}
