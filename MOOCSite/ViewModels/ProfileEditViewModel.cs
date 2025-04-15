// ViewModels/ProfileEditViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MOOCSite.ViewModels
{
    public class ProfileEditViewModel
    {
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; }
    }
}