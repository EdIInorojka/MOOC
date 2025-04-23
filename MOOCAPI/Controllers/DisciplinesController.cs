using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOOCAPI.Data;
using MOOCAPI.Models;

namespace MOOCAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisciplinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DisciplinesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Discipline>>> GetDisciplines()
        {
            return await _context.Disciplines.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Discipline>> GetDiscipline(int id)
        {
            var discipline = await _context.Disciplines.FindAsync(id);
            if (discipline == null) return NotFound();
            return discipline;
        }

        [HttpPost]
        public async Task<ActionResult<Discipline>> CreateDiscipline(Discipline discipline)
        {
            _context.Disciplines.Add(discipline);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDiscipline), new { id = discipline.Id }, discipline);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiscipline(int id, Discipline discipline)
        {
            if (id != discipline.Id) return BadRequest();
            _context.Entry(discipline).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscipline(int id)
        {
            var discipline = await _context.Disciplines.FindAsync(id);
            if (discipline == null) return NotFound();
            _context.Disciplines.Remove(discipline);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/courses")]
        public async Task<ActionResult<IEnumerable<Course>>> GetDisciplineCourses(int id)
        {
            var discipline = await _context.Disciplines
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (discipline == null) return NotFound();
            return Ok(discipline.Courses);
        }
    }
}
