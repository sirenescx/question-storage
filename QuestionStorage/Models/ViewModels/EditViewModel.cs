using System.ComponentModel.DataAnnotations;

namespace QuestionStorage.Models.ViewModels
{
    public class EditViewModel
    {
        [Required]
        [MinLengthAttribute(8, ErrorMessage = "Password should contain at least 8 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}