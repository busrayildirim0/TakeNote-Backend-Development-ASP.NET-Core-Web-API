// TakeNote.Service/DTOs/Workspace/WorkspaceUpdateDto.cs - GÜNCELLENMİŞ
namespace TakeNote.Service.DTOs
{
    public class WorkspaceUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool? IsPrivate { get; set; } // YENİ: Private/Public geçişi
    }
}