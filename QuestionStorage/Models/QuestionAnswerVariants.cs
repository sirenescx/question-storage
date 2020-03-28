using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models
{
    public partial class QuestionAnswerVariants
    {
        public QuestionAnswerVariants()
        {
            QuizesInfoQuestionsInfoQuestionAnswerVariants = new HashSet<QuizesInfoQuestionsInfoQuestionAnswerVariants>();
        }

        [Key]
        [Column("VariantID")]
        public int VariantId { get; set; }
        [Column("QuestID")]
        public int QuestId { get; set; }
        public int SortCode { get; set; }
        [StringLength(256)]
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }

        [ForeignKey(nameof(QuestId))]
        [InverseProperty(nameof(QuestionsInfo.QuestionAnswerVariants))]
        public virtual QuestionsInfo Quest { get; set; }
        [InverseProperty("Variant")]
        public virtual ICollection<QuizesInfoQuestionsInfoQuestionAnswerVariants> QuizesInfoQuestionsInfoQuestionAnswerVariants { get; set; }
    }
}
