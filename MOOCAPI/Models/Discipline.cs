using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MOOCAPI.Models
{
    public class Discipline
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Column(TypeName = "date")]
        public DateOnly StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateOnly EndDate { get; set; }

        [Range(0, 100)]
        public int Credits { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
