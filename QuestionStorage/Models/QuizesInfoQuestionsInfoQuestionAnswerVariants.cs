using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models
{
    [Table("QuizesInfo_QuestionsInfo_QuestionAnswerVariants")]
    public partial class QuizesInfoQuestionsInfoQuestionAnswerVariants
    {
        [Key]
        [Column("QuizID")]
        public int QuizId { get; set; }
        [Key]
        [Column("VariantID")]
        public int VariantId { get; set; }
        public int? SortCode { get; set; }

        [ForeignKey(nameof(QuizId))]
        [InverseProperty(nameof(QuizesInfo.QuizesInfoQuestionsInfoQuestionAnswerVariants))]
        public virtual QuizesInfo Quiz { get; set; }
        [ForeignKey(nameof(VariantId))]
        [InverseProperty(nameof(QuestionAnswerVariants.QuizesInfoQuestionsInfoQuestionAnswerVariants))]
        public virtual QuestionAnswerVariants Variant { get; set; }
    }
}
