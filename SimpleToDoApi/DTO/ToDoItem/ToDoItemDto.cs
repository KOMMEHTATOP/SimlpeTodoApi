using SimpleToDoApi.Models;
using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.ToDoItem;

public class ToDoItemDto
{
    public string Id { get; set; }
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")] 
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsComplete { get; set; }
}
