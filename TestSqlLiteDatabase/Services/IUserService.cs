using TestSqlLiteDatabase.Model;

namespace TestSqlLiteDatabase.Services
{
    public interface IUserService
    {
        public Task<User> AddUser(User user);
        public Task<IEnumerable<User>> GetAllUsers();
        public Task<User> GetUserById(int id);
        public Task<User> UpdateUser(int id, string name);
        public Task<User> DeleteUser(int id);

    }
}
