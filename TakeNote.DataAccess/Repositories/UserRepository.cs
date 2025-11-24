using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;

namespace TakeNote.DataAccess.Repositories
{
    // EfRepository'den User için miras alıyoruz, aynı zamanda IUserRepository sözleşmesini imzalıyoruz.
    public class UserRepository : EfRepository<User>, IUserRepository
    {
        // Constructor: Context'i alıp babasına (EfRepository) gönderiyor.
        public UserRepository(AppDbContext context) : base(context)
        {
        }
    }
}