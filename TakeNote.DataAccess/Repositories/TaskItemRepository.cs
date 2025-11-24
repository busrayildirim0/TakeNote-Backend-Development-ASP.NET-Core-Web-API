using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;

namespace TakeNote.DataAccess.Repositories
{
    public class TaskItemRepository : EfRepository<TaskItem>, ITaskItemRepository
    {
        public TaskItemRepository(AppDbContext context) : base(context)
        {
        }
    }
}