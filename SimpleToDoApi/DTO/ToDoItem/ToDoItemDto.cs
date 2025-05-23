using SimpleToDoApi.Models;
using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.ToDoItem;

public class ToDoItemDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsComplete { get; set; }
}
