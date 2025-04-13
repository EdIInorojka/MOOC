using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MOOCSite.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Link { get; set; }
        public string? Description { get; set; }
        public bool IsSelfPassed { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Price { get; set; }
        public string? Language { get; set; }
        public bool Certificated { get; set; }
        public bool IsCertificatePaid { get; set; }
        public int Credits { get; set; }
        public float Reviews { get; set; }
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
