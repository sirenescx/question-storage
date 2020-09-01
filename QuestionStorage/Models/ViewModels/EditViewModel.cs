using System.ComponentModel.DataAnnotations;
using QuestionStorage.Utils;

namespace QuestionStorage.Models.ViewModels
{
    public class EditViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        
        [Required]
        [MinLengthAttribute(8, ErrorMessage = "Password should contain at least 8 characters")]
        [DataType(DataType.Password)]
        [NotEqual("OldPassword")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}