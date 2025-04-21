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

        [HttpPost("{userId}/password")]
        public IActionResult ChangePassword(int userId, [FromBody] PasswordChangeDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            if (user.Password != dto.CurrentPassword) // Простое сравнение без хеширования
                return BadRequest("Неверный текущий пароль");

            user.Password = dto.NewPassword; // Сохраняем как есть
            _context.SaveChanges();

            return Ok();
        }

        public class PasswordChangeDto
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }

        [HttpPost("{userId}/courses/{courseId}")]
        public async Task<IActionResult> AddCourseToUser(int userId, int courseId)
        {
            var user = await _context.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var course = await _context.Courses.FindAsync(courseId);

            if (user == null || course == null)
            {
                return NotFound("User or course not found");
            }

            if (user.Courses.Any(c => c.Id == courseId))
            {
                return Conflict("User already enrolled in this course");
            }

            user.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(IsUserEnrolled), new { userId, courseId }, null);
        }

        [HttpGet("{userId}/courses/{courseId}/isEnrolled")]
        public async Task<ActionResult<bool>> IsUserEnrolled(int userId, int courseId)
        {
            var user = await _context.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found");

            return Ok(user.Courses.Any(c => c.Id == courseId));
        }

        [HttpGet("{userId}/courses")]
        public async Task<ActionResult<IEnumerable<Course>>> GetUserCourses(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Courses) // Важно: включаем связанные курсы
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found");

            return Ok(user.Courses);
        }

        [HttpDelete("{userId}/courses/{courseId}")]
        public async Task<IActionResult> RemoveCourseFromUser(int userId, int courseId)
        {
            var user = await _context.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found");

            var course = user.Courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null) return NotFound("Course not found in user's courses");

            user.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("courses/{courseId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByCourse(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound("Course not found");

            return Ok(course.Users);
        }

        [HttpGet("{userId}/courses/count")]
        public async Task<ActionResult<int>> GetUserCoursesCount(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found");

            return Ok(user.Courses.Count);
        }

        [HttpGet("{userId}/courses/{courseId}/canGetCertificate")]
        public async Task<ActionResult<bool>> CanGetCertificate(int userId, int courseId)
        {
            var user = await _context.Users
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found");

            var course = user.Courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null) return NotFound("User is not enrolled in this course");

            // Логика проверки возможности получения сертификата
            bool canGetCertificate = course.Certificated &&
                                   (!course.IsCertificatePaid || /* проверка оплаты */ true);

            return Ok(canGetCertificate);
        }
    }

    public class LoginModel
    {
        public string LoginOrEmail { get; set; }
        public string Password { get; set; }
    }
}
