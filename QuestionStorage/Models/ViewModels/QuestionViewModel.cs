using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.ViewModels
{
    public class QuestionViewModel 
    {
        public Question Question { get; set; }
        public AnswerOption AnswerOption { get; set; }
    }
}