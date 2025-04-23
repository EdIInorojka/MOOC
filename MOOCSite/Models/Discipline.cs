using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MOOCSite.Models
{
    public class Discipline
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Credits { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}