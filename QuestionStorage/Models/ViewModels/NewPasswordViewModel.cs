using System.ComponentModel.DataAnnotations;
using QuestionStorage.Utils;

namespace QuestionStorage.Models.ViewModels
{
    public class NewPasswordViewModel : EditViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [NotEqual("OldPassword")]
        public override string NewPassword { get; set; }
    }
}