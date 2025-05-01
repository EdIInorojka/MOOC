using MOOCSite.Models;
using System.ComponentModel.DataAnnotations;

namespace MOOCSite.ViewModels
{
    public class CourseCreateViewModel
    {
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов")]
        public string Title { get; set; }

        [Url(ErrorMessage = "Некорректный URL")]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов")]
        public string? Link { get; set; }

        [StringLength(2000, ErrorMessage = "Максимальная длина 2000 символов")]
        public string? Description { get; set; }

        [Display(Name = "Самостоятельное прохождение")]
        public bool IsSelfPassed { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        [Display(Name = "Дата начала")]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Дата окончания обязательна")]
        [Display(Name = "Дата окончания")]
        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

        [Range(0, 1000000, ErrorMessage = "Цена должна быть от 0 до 1 000 000")]
        public int Price { get; set; }

        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов")]
        public string? Language { get; set; }

        [Display(Name = "Выдаётся сертификат")]
        public bool Certificated { get; set; }

        [Display(Name = "Сертификат платный")]
        public bool IsCertificatePaid { get; set; }

        [Range(0, 1000, ErrorMessage = "Кредиты должны быть от 0 до 1000")]
        public int Credits { get; set; }

        [Range(0, 5, ErrorMessage = "Рейтинг должен быть от 0 до 5")]
        public float Reviews { get; set; }

        [Display(Name = "Университет")]
        public int? UniversityId { get; set; }

        [Display(Name = "Дисциплины")]
        public List<int> SelectedDisciplineIds { get; set; } = new();

        [Display(Name = "Лекторы")]
        public List<int> SelectedLecturerIds { get; set; } = new();

        // Списки для выбора
        public List<University>? Universities { get; set; }
        public List<Discipline>? Disciplines { get; set; }
        public List<Lecturer>? Lecturers { get; set; }
    }
}
