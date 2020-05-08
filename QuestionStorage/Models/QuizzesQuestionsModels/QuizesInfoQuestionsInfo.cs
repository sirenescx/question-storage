using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    [Table("QuizesInfo_QuestionsInfo")]
    public partial class QuizesInfoQuestionsInfo
    {
        [Key]
        [Column("QuizID")]
        public int QuizId { get; set; }
        [Key]
        [Column("QuestID")]
        public int QuestId { get; set; }
        [StringLength(16)]
        public string CodeSort { get; set; }

        [ForeignKey(nameof(QuestId))]
        [InverseProperty(nameof(QuestionsInfo.QuizesInfoQuestionsInfo))]
        public virtual QuestionsInfo Quest { get; set; }
        [ForeignKey(nameof(QuizId))]
        [InverseProperty(nameof(QuizesInfo.QuizesInfoQuestionsInfo))]
        public virtual QuizesInfo Quiz { get; set; }
    }
}
