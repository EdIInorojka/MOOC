using System.ComponentModel.DataAnnotations;

namespace MOOCSite.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Логин или Email обязателен")]
        public string LoginOrEmail { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; }
    }
}
