using System.ComponentModel.DataAnnotations;
using QuestionStorage.Helpers;

namespace QuestionStorage.Models.ViewModels
{
    public class NewPasswordViewModel : ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [NotEqual("OldPassword")]
        public override string NewPassword { get; set; }
    }
}