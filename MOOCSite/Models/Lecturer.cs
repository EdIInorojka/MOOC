using System.ComponentModel.DataAnnotations;

namespace MOOCSite.Models
{
    public class Lecturer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{LastName} {FirstName}";

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
