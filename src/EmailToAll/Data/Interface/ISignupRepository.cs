using EmailToAll.Models;

namespace EmailToAll.Data.Interface
{
    public interface ISignupRepository
    {
        Task <IEnumerable<Signup>> GetAlllAsync();
        Task AddAsync (Signup signup);
        Task UpdateAsync (Signup signup);
        Task DeleteAsync(int id);
        Task <Signup> GetByIdAsync(int id);
    }
}
