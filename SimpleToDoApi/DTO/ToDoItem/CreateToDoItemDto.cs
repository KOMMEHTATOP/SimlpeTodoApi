using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.DTO.ToDoItem;

public class CreateToDoItemDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")] 
    public string Title { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public bool IsComplete { get; set; }
}
