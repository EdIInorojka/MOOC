using MOOCSite.Models;
namespace MOOCSite.ViewModels
{
    public class CourseWithEnrollmentViewModel
    {
        public Course Course { get; set; }
        public bool IsEnrolled { get; set; }
    }
}
