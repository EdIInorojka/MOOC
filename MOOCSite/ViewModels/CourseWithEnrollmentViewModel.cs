using MOOCSite.Models;
namespace MOOCSite.ViewModels
{
    public class CourseWithEnrollmentViewModel
    {
        public Course Course { get; set; }
        public bool IsEnrolled { get; set; }
        public University University { get; set; }
        public List<Lecturer> Lecturers { get; set; } = new List<Lecturer>();
    }
}
