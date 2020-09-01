using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Quizzes
{
    [Table("QuizzesInfo_QuestionsInfo_QuestionAnswerVariants")]
    public class QuizzesInfoQuestionsInfoQuestionAnswerVariants
    {
        [Key]
        [Column("QuizID")]
        public int QuizId { get; set; }
        [Key]
        [Column("VariantID")]
        public int VariantId { get; set; }
        [Column("SortCode")]
        public int? SortCode { get; set; }
        [ForeignKey(nameof(QuizId))]
        [InverseProperty(nameof(QuizzesInfo.QuizzesInfoQuestionsInfoQuestionAnswerVariants))]
        public virtual QuizzesInfo Quiz { get; set; }
        [ForeignKey(nameof(VariantId))]
        [InverseProperty(nameof(QuestionAnswerVariants.QuizzesInfoQuestionsInfoQuestionAnswerVariants))]
        public virtual QuestionAnswerVariants Variant { get; set; }
    }
}