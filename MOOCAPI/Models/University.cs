using System.ComponentModel.DataAnnotations;

namespace MOOCAPI.Models
{
    public class University
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
