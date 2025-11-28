namespace TakeNote.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        
        IWorkspaceRepository Workspaces { get; }
        INoteRepository Notes { get; }
        ITaskItemRepository TaskItems { get; }
        // proje ilerledikçe yenileri eklenecek.
        Task<int> CompleteAsync();
    }
}