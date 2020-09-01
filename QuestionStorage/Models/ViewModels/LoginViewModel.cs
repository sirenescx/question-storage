using System.ComponentModel.DataAnnotations;

namespace QuestionStorage.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-mail is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
         
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}