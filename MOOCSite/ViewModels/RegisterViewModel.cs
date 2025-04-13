using System.ComponentModel.DataAnnotations;

namespace MOOCSite.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Логин обязателен")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string? Email { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
