namespace TakeNote.Service.DTOs
{
    public class NoteQueryDto
    {
        public int? WorkspaceId { get; set; }
        public string? Search { get; set; } // Başlık veya içerikte ara
        public string? SortBy { get; set; } // "DateDesc", "TitleAsc" vb.
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}