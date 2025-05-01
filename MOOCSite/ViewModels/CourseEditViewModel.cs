using MOOCSite.Models;

namespace MOOCSite.ViewModels
{
    public class CourseEditViewModel
    {
        public Course Course { get; set; }
        public List<University> Universities { get; set; } = new List<University>();
        public List<Lecturer> Lecturers { get; set; } = new List<Lecturer>();
        public List<Discipline> Disciplines { get; set; } = new List<Discipline>();
        public List<int> SelectedLecturerIds { get; set; } = new List<int>();
        public List<int> SelectedDisciplineIds { get; set; } = new List<int>();
    }
}
