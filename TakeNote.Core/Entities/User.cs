using Microsoft.AspNetCore.Identity;

namespace TakeNote.Core.Entities
{
    // IdentityUser'dan miras alıyoruz (Id, Username, Email, PasswordHash otomatik geliyor)
    public class User : IdentityUser<Guid>
    {
        // Ekstra istediğin özellik varsa buraya yazabilirsin (Örn: Ad Soyad)
        public ICollection<WorkspaceMember> Workspaces { get; set; } = new List<WorkspaceMember>();
    }

    // UserRole sınıfına gerek kalmayabilir, IdentityRole kullanacağız.
}