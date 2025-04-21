using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOOCAPI.Data;
using MOOCAPI.Models;
using System.Linq.Dynamic.Core;

namespace MOOCAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly List<string> _sortableFields = new List<string>
    {
        "title", "link", "isSelfPassed", "startDate", "endDate",
        "price", "language", "certificated", "isCertificatePaid",
        "credits", "reviews"
    };

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses(
            string sortBy = "title",
            string sortOrder = "asc",
            string search = "")
        {
            IQueryable<Course> query = _context.Courses;

            // Поиск по названию и языку
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Title.Contains(search) ||
                    c.Language.Contains(search));
            }

            // Проверка допустимости поля для сортировки
            sortBy = _sortableFields.Contains(sortBy.ToLower()) ? sortBy : "title";
            sortOrder = sortOrder.ToLower() == "desc" ? "desc" : "asc";

            // Динамическая сортировка
            query = query.OrderBy($"{sortBy} {sortOrder}");

            return await query.ToListAsync();
        }

        // GET: api/Courses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return course;
        }

        // POST: api/Courses
        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse(Course course)
        {
            if (await _context.Courses.AnyAsync(c => c.Title == course.Title))
            {
                return BadRequest("Course with this title already exists");
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        // PUT: api/Courses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, Course course)
        {
            if (id != course.Id)
            {
                return BadRequest();
            }

            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            if (await _context.Courses.AnyAsync(c => c.Id != id && c.Title == course.Title))
            {
                return BadRequest("Course with this title already exists");
            }

            _context.Entry(existingCourse).CurrentValues.SetValues(course);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        [HttpGet("Filter")]
        public async Task<ActionResult<IEnumerable<Course>>> FilterCourses(
            string search = "",
            string sortBy = "title",
            string sortOrder = "asc",
            bool? isSelfPassed = null,
            bool? certificated = null,
            string language = null,
            int? minPrice = null,
            int? maxPrice = null,
            float? minRating = null)
        {
            IQueryable<Course> query = _context.Courses;

            // Поиск
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }

            // Фильтрация
            if (isSelfPassed.HasValue)
            {
                query = query.Where(c => c.IsSelfPassed == isSelfPassed);
            }

            if (certificated.HasValue)
            {
                query = query.Where(c => c.Certificated == certificated);
            }

            if (!string.IsNullOrEmpty(language))
            {
                query = query.Where(c => c.Language == language);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= maxPrice);
            }

            if (minRating.HasValue)
            {
                query = query.Where(c => c.Reviews >= minRating);
            }

            // Сортировка
            switch (sortBy.ToLower())
            {
                case "title":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.Title) : query.OrderByDescending(c => c.Title);
                    break;
                case "startdate":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.StartDate) : query.OrderByDescending(c => c.StartDate);
                    break;
                case "price":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.Price) : query.OrderByDescending(c => c.Price);
                    break;
                case "reviews":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.Reviews) : query.OrderByDescending(c => c.Reviews);
                    break;
                default:
                    query = query.OrderBy(c => c.Title);
                    break;
            }

            return await query.ToListAsync();
        }
    }
}
