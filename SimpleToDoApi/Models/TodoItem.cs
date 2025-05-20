using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Может понадобиться для ForeignKey

namespace SimpleToDoApi.Models // Убедись, что ApplicationUser доступен в этом namespace или добавь нужный using
{
    public class ToDoItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")]
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
