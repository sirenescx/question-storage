using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Quizzes
{
    public class QuizzesInfoQuestionsInfo
    {
        public int QuizId { get; set; }
        public int QuestId { get; set; }
        public string CodeSort { get; set; }
        [ForeignKey(nameof(QuizId))]
        [InverseProperty(nameof(QuestionsInfo.QuizzesInfoQuestionsInfo))]
        public virtual QuestionsInfo Quest { get; set; }
        [ForeignKey(nameof(QuestId))]
        [InverseProperty(nameof(QuizzesInfo.QuizzesInfoQuestionsInfo))]
        public virtual QuizzesInfo Quiz { get; set; }
    }
}
