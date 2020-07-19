﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    public class QuestionAnswerVariants
    {
        [Key]
        [Column("VariantID")]
        public int VariantId { get; set; }
        [Column("QuestID")]
        public int QuestId { get; set; }
        public int SortCode { get; set; }
        [StringLength(256)]
        [Required(ErrorMessage = "Response option text is required.")]
        [MinLengthAttribute(1)]
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }

        [ForeignKey(nameof(QuestId))]
        [InverseProperty(nameof(QuestionsInfo.QuestionAnswerVariants))]
        public virtual QuestionsInfo Quest { get; set; }
    }
}
