using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOOCAPI.Data;
using MOOCAPI.Models;

namespace MOOCAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UniversitiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Universities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<University>>> GetUniversities()
        {
            return await _context.Universities.ToListAsync();
        }

        // GET: api/Universities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<University>> GetUniversity(int id)
        {
            var university = await _context.Universities.FindAsync(id);
            if (university == null) return NotFound();
            return university;
        }

        // POST: api/Universities
        [HttpPost]
        public async Task<ActionResult<University>> PostUniversity(University university)
        {
            _context.Universities.Add(university);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUniversity), new { id = university.Id }, university);
        }

        // PUT: api/Universities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUniversity(int id, University university)
        {
            if (id != university.Id) return BadRequest();

            _context.Entry(university).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Universities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUniversity(int id)
        {
            var university = await _context.Universities.FindAsync(id);
            if (university == null) return NotFound();

            _context.Universities.Remove(university);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
