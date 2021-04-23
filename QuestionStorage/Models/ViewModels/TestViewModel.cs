using System.ComponentModel.DataAnnotations;
using QuestionStorage.Models.Quizzes;

namespace QuestionStorage.Models.ViewModels
{
    public class TestViewModel : Quiz
    {
        [Range(1, int.MaxValue)]
        public string QuestionId { get; set; }
    }
}

