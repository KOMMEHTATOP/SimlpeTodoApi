using System.ComponentModel.DataAnnotations;

namespace SimpleToDoApi.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")] 
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; private set; } = DateTime.UtcNow;
        public DateTime Updated {get; set;}
        
        //Связь между User и ToDoItem
        public int CreatedByUserId {get; set;}
        public User CreatedByUser {get; set;}
        public bool IsComplete { get; set; }
    }
}
