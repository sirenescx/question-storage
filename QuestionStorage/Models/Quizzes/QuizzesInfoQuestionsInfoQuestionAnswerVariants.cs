using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Quizzes
{
    public class QuizzesInfoQuestionsInfoQuestionAnswerVariants
    {
        public int QuizId { get; set; }
        public int VariantId { get; set; }
        public int? SortCode { get; set; }
        [ForeignKey(nameof(QuizId))]
        [InverseProperty(nameof(QuizzesInfo.QuizzesInfoQuestionsInfoQuestionAnswerVariants))]
        public virtual QuizzesInfo Quiz { get; set; }
        [ForeignKey(nameof(VariantId))]
        [InverseProperty(nameof(QuestionAnswerVariants.QuizzesInfoQuestionsInfoQuestionAnswerVariants))]
        public virtual QuestionAnswerVariants Variant { get; set; }
    }
}
