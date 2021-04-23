using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Quizzes
{
    public class QuizzesAnswerOptions
    {
        public int QuizId { get; set; }
        public int AnswerOptionId { get; set; }
        
        public virtual Quiz Quiz{ get; set; }
        public virtual AnswerOption Option { get; set; }
    }
}