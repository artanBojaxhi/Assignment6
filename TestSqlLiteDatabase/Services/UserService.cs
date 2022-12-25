using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Xml.Linq;
using TestSqlLiteDatabase.Data;
using TestSqlLiteDatabase.Model;

namespace TestSqlLiteDatabase.Services
{
    public class UserService : IUserService
    {
        ApplicationDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        ConnectionMultiplexer _connectionMultiplexer;

        public UserService(ApplicationDbContext dbContext, IMemoryCache memoryCache, ConnectionMultiplexer connectionMultiplexer)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _connectionMultiplexer = connectionMultiplexer;
        }
        
        public async Task<User> AddUser(User user)
        {
            var addedUser = await _dbContext.Users.AddAsync(new User { Id = user.Id, Name = user.Name });
            await _dbContext.SaveChangesAsync();
            _memoryCache.Set("user" + user.Id, addedUser.Entity, TimeSpan.FromMinutes(1));
            return addedUser.Entity;
        }
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            if (_memoryCache.TryGetValue("users", out List<User> users))
            {
                return users;
            }

            var db = _connectionMultiplexer.GetDatabase();

            var cacheKey = "users";
            var cachedUsers = db.StringGet(cacheKey);
            if (cachedUsers.HasValue)
            {
                users = JsonConvert.DeserializeObject<List<User>>(cachedUsers);


                _memoryCache.Set("users", users, TimeSpan.FromMinutes(1));

                return users;
            }
            users = await _dbContext.Users.ToListAsync();

            _memoryCache.Set("users", users, TimeSpan.FromMinutes(1));

            db.StringSet(cacheKey, JsonConvert.SerializeObject(users), TimeSpan.FromMinutes(1));

            return users;
        }
        public async Task<User> GetUserById(int id)
        {
            if(_memoryCache.TryGetValue("user", out User user))
            {
                return user;
            }
           
            var db = _connectionMultiplexer.GetDatabase();
            var cacheKey = "user";
            var cachedUsers = db.StringGet(cacheKey);
            if (cachedUsers.HasValue)
            {
                user = JsonConvert.DeserializeObject<User>(cachedUsers);

                _memoryCache.Set("user", user, TimeSpan.FromMinutes(1));

                return user;
            }
            user = await _dbContext.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
            _memoryCache.Set("user", user, TimeSpan.FromMinutes(1));
            db.StringSet(cacheKey, JsonConvert.SerializeObject(user), TimeSpan.FromMinutes(1));
            return user;
        }
        public async Task<User> UpdateUser(int id, string name)
        {
            if ((_dbContext.Users.Where(u => u.Id == id).Count() == 0))
            {
                throw new NullReferenceException();
            }

            
            var db = _connectionMultiplexer.GetDatabase();

            var user = _dbContext.Users.Where(u => u.Id == id).First();
            user.Name = name;
            await _dbContext.SaveChangesAsync();

            _memoryCache.Set("user" + id, user, TimeSpan.FromMinutes(1));
            db.StringSet("user" + id, JsonConvert.SerializeObject(user), TimeSpan.FromMinutes(1));

            return user;
        }
        public async Task<User> DeleteUser(int id)
        {
            if ((_dbContext.Users.Where(u => u.Id == id).Count() == 0))
            {
                throw new NullReferenceException();
            }

           
            var db = _connectionMultiplexer.GetDatabase();

            _memoryCache.Remove("user" + id);

            db.KeyDelete("user" + id);

            var entity = _dbContext.Users.Where(u => u.Id == id).First();
            _dbContext.Users.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }
    }
}
