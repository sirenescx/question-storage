using QuestionStorage.Models.QuizzesQuestionsModels;

namespace QuestionStorage.Models.ViewModels
{
    public class TemplateQuestionViewModel : QuestionViewModel
    {
        public string Code { get; set; }
        public int Amount { get; set; }
    }
    
}