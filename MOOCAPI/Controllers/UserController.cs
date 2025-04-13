using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOOCAPI.Data;
using MOOCAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MOOCAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Login == user.Login))
            {
                return BadRequest("Пользователь с таким логином уже существует");
            }

            if (!string.IsNullOrEmpty(user.Email) && await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Пользователь с такой почтой уже существует");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (await _context.Users.AnyAsync(u => u.Id != id && u.Login == user.Login))
            {
                return BadRequest("Пользователь с таким логином уже существует");
            }

            if (!string.IsNullOrEmpty(user.Email) &&
                await _context.Users.AnyAsync(u => u.Id != id && u.Email == user.Email))
            {
                return BadRequest("Пользователь с такой почтой уже существует");
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Users/Authenticate
        [HttpPost("Authenticate")]
        public async Task<ActionResult<User>> Authenticate(LoginModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    (u.Login == model.LoginOrEmail || u.Email == model.LoginOrEmail) &&
                    u.Password == model.Password);

            if (user == null)
            {
                return Unauthorized("Неверный логин/почта или пароль");
            }

            // Возвращаем пользователя без пароля в реальном проекте
            return Ok(new
            {
                user.Id,
                user.Login,
                user.Email,
                user.FirstName,
                user.LastName
            });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }

    public class LoginModel
    {
        public string LoginOrEmail { get; set; }
        public string Password { get; set; }
    }
}
