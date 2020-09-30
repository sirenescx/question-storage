using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Quizzes;

namespace QuestionStorage.Models.Questions
{
    public class QuestionAnswerVariants
    {
        public QuestionAnswerVariants()
        {
            QuizzesInfoQuestionsInfoQuestionAnswerVariants = new HashSet<QuizzesInfoQuestionsInfoQuestionAnswerVariants>();
        }

        public int VariantId { get; set; }
        public int QuestId { get; set; }
        public int SortCode { get; set; }
        [StringLength(256)]
        [Required(ErrorMessage = "Response option text is required.")]
        [MinLength(1)]
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        [ForeignKey(nameof(QuestId))]
        [InverseProperty(nameof(QuestionsInfo.QuestionAnswerVariants))]
        public virtual QuestionsInfo Quest { get; set; }
        public virtual ICollection<QuizzesInfoQuestionsInfoQuestionAnswerVariants> QuizzesInfoQuestionsInfoQuestionAnswerVariants { get; set; }
    }
}
