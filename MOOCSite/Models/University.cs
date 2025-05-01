using System.ComponentModel.DataAnnotations;

namespace MOOCSite.Models
{
    public class University
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
