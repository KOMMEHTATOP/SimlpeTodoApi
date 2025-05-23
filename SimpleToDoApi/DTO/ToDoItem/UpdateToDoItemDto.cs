using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.ToDoItem;

public class UpdateToDoItemDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsComplete { get; set; }
}
