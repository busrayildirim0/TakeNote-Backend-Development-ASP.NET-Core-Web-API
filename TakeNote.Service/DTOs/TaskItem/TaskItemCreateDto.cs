using System.ComponentModel.DataAnnotations;

namespace TakeNote.Service.DTOs
{
    public class TaskItemCreateDto
    {
        [Required]
        public int NoteId { get; set; } // Hangi notun içinde bu görev?

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }
        public Guid? AssignedToId { get; set; } // Kime atandı?
    }
}