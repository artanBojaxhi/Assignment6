using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestSqlLiteDatabase.Data;
using TestSqlLiteDatabase.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using TestSqlLiteDatabase.Services;

namespace TestSqlLiteDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(User user)
        {
            var createdUser = await _userService.AddUser(user);
            return Ok(createdUser);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<User>> Update(int id, string name)
        {
            try
            {
                var updatedUser = await _userService.UpdateUser(id, name);
                return Ok(updatedUser);
            }
            catch (NullReferenceException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> Delete(int id)
        {
            try
            {
                var deletedUser = await _userService.DeleteUser(id);
                return Ok(deletedUser);
            }
            catch (NullReferenceException)
            {
                return NotFound();
            }
        }
    }
}
