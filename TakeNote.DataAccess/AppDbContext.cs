using Microsoft.EntityFrameworkCore;
using TakeNote.Core.Entities;

namespace TakeNote.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tablolarımız
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Many-to-Many İlişki Ayarı (Fluent API)
            modelBuilder.Entity<WorkspaceMember>()
                .HasKey(wm => new { wm.UserId, wm.WorkspaceId }); // İkisi birleşip Primary Key oluyor

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.User)
                .WithMany(u => u.Workspaces)
                .HasForeignKey(wm => wm.UserId);

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId);

            base.OnModelCreating(modelBuilder);
        }
    }
}