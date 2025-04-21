// ViewModels/ProfileEditViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MOOCSite.ViewModels
{
    public class EditProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
        public string Login { get; set; }

        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        public string? PhoneNumber { get; set; }
    }
}