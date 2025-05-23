using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleToDoApi.Models 
{
    public class ToDoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; private set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; }
        public bool IsComplete { get; set; }

        // Связь между ApplicationUser (Identity) и ToDoItem
        [Required] // Скорее всего, ToDoItem должен иметь создателя
        public string CreatedByUserId { get; set; } // <--- ИЗМЕНЕН ТИП НА STRING

        [ForeignKey("CreatedByUserId")] // Явно указываем внешний ключ
        public ApplicationUser CreatedByUser { get; set; } // <--- ИЗМЕНЕН ТИП НА ApplicationUser
    }
}
