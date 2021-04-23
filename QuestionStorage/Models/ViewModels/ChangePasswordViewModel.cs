using System.ComponentModel.DataAnnotations;

namespace QuestionStorage.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [MinLengthAttribute(8, ErrorMessage = "Password should contain at least 8 characters")]
        [DataType(DataType.Password)]
        public virtual string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmationPassword { get; set; }
    }
}