namespace TakeNote.Service.DTOs
{
    public class NoteUpdateDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsPinned { get; set; }
        public bool? IsLocked { get; set; }
    }
}