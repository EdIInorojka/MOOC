﻿using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("{id}/disciplines")]
        public async Task<ActionResult<IEnumerable<Discipline>>> GetCourseDisciplines(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Disciplines)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();
            return Ok(course.Disciplines);
        }

        [HttpPost("{id}/disciplines/{disciplineId}")]
        public async Task<IActionResult> AddDisciplineToCourse(int id, int disciplineId)
        {
            var course = await _context.Courses
                .Include(c => c.Disciplines)
                .FirstOrDefaultAsync(c => c.Id == id);

            var discipline = await _context.Disciplines.FindAsync(disciplineId);

            if (course == null || discipline == null) return NotFound();

            course.Disciplines.Add(discipline);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}/disciplines/{disciplineId}")]
        public async Task<IActionResult> RemoveDisciplineFromCourse(int id, int disciplineId)
        {
            var course = await _context.Courses
                .Include(c => c.Disciplines)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var discipline = course.Disciplines.FirstOrDefault(d => d.Id == disciplineId);
            if (discipline == null) return NotFound();

            course.Disciplines.Remove(discipline);
            await _context.SaveChangesAsync();

            return NoContent();
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
            float? minRating = null,
            int? disciplineId = null,
            int? universityId = null,
            int? lecturerId = null,
            string universityName = null,
            string lecturerName = null)
        {
            IQueryable<Course> query = _context.Courses
                .Include(c => c.Disciplines)
                .Include(c => c.University)
                .Include(c => c.Lecturers)
                .Include(c => c.Users)
                .AsQueryable();

            // Фильтрация по дисциплине (исправленная версия)

            // Остальные фильтры
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
            }

            if (universityId.HasValue)
            {
                query = query.Where(c => c.UniversityId == universityId.Value);
            }
            if (!string.IsNullOrEmpty(universityName))
            {
                query = query.Where(c => c.University != null && c.University.Name.Contains(universityName));
            }

            if (lecturerId.HasValue)
            {
                query = query.Where(c => c.Lecturers.Any(l => l.Id == lecturerId.Value));
            }
            if (!string.IsNullOrEmpty(lecturerName))
            {
                query = query.Where(c => c.Lecturers.Any(l =>
                    (l.FirstName + " " + l.LastName).Contains(lecturerName)));
            }

            if (isSelfPassed.HasValue)
            {
                query = query.Where(c => c.IsSelfPassed == isSelfPassed.Value);
            }

            if (certificated.HasValue)
            {
                query = query.Where(c => c.Certificated == certificated.Value);
            }

            if (!string.IsNullOrEmpty(language))
            {
                query = query.Where(c => c.Language == language);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= maxPrice.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(c => c.Reviews >= minRating.Value);
            }

            if (disciplineId.HasValue)
            {
                query = query.Where(c => c.Disciplines.Any(d => d.Id == disciplineId.Value));
            }

            // Сортировка
            switch (sortBy.ToLower())
            {
                case "title":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.Title) : query.OrderByDescending(c => c.Title);
                    break;
                case "price":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.Price) : query.OrderByDescending(c => c.Price);
                    break;
                case "reviews":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.Reviews) : query.OrderByDescending(c => c.Reviews);
                    break;
                case "startdate":
                    query = sortOrder == "asc" ? query.OrderBy(c => c.StartDate) : query.OrderByDescending(c => c.StartDate);
                    break;
                default:
                    query = query.OrderBy(c => c.Title);
                    break;
            }

            return await query.ToListAsync();
        }

        [HttpGet("ByDiscipline/{disciplineId}")]
        public async Task<ActionResult<IEnumerable<Course>>> GetCoursesByDiscipline(int disciplineId)
        {
            var discipline = await _context.Disciplines
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(d => d.Id == disciplineId);

            if (discipline == null) return NotFound();

            return Ok(discipline.Courses);
        }

        [HttpPost("{courseId}/lecturers/{lecturerId}")]
        public async Task<IActionResult> AddLecturerToCourse(int courseId, int lecturerId)
        {
            var course = await _context.Courses.Include(c => c.Lecturers).FirstOrDefaultAsync(c => c.Id == courseId);
            var lecturer = await _context.Lecturers.FindAsync(lecturerId);

            if (course == null || lecturer == null) return NotFound();

            course.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
