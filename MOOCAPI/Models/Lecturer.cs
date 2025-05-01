using System.ComponentModel.DataAnnotations;

namespace MOOCAPI.Models
{
    public class Lecturer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
