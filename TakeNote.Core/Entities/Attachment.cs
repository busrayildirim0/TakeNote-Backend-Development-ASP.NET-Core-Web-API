namespace TakeNote.Core.Entities
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty; // "image/png", "application/pdf"
        public string Url { get; set; } = string.Empty; // Dosyanın Cloud veya Disk yolu
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int NoteId { get; set; }
        public Note Note { get; set; } = null!;

        public Guid UploadedById { get; set; }
        public User UploadedBy { get; set; } = null!;
    }
}