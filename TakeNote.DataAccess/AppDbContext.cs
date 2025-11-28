using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TakeNote.Core.Entities;

namespace TakeNote.DataAccess
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        // YENİ EKLENENLER:
        public DbSet<Note> Notes { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. WorkspaceMember (Many-to-Many)
            modelBuilder.Entity<WorkspaceMember>()
                .HasKey(wm => new { wm.UserId, wm.WorkspaceId });

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.User)
                .WithMany(u => u.Workspaces)
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Döngüsel silmeyi engellemek için

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Note Config
            modelBuilder.Entity<Note>()
                .HasOne(n => n.Workspace)
                .WithMany(w => w.Notes)
                .HasForeignKey(n => n.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade); // Workspace silinirse notlar da silinsin

            modelBuilder.Entity<Note>()
                .HasOne(n => n.CreatedBy)
                .WithMany()
                .HasForeignKey(n => n.CreatedById)
                .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse notlar kalsın (veya silinsin size bağlı)

            // 3. Attachment Config
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Note)
                .WithMany(n => n.Attachments)
                .HasForeignKey(a => a.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. TaskItem Config
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Note)
                .WithMany(n => n.Tasks)
                .HasForeignKey(t => t.NoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}