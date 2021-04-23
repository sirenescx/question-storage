using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Quizzes
{
    public class QuizzesQuestions
    {
        public int QuizId { get; set; }
        public int QuestionId { get; set; }
        public virtual Quiz Quiz { get; set; }
        public virtual Question Question { get; set; }
    }
}