using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MOOCAPI.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Url]
        [MaxLength(500)]
        public string? Link { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public bool IsSelfPassed { get; set; }

        [Column(TypeName = "date")]
        public DateOnly StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateOnly EndDate { get; set; }

        [Range(0, 1000000)]
        public int Price { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; }

        public bool Certificated { get; set; }

        public bool IsCertificatePaid { get; set; }

        [Range(0, 1000)]
        public int Credits { get; set; }

        [Range(0, 5)]
        public float Reviews { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
