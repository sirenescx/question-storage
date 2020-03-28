using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models
{
    public partial class QuizesInfo
    {
        public QuizesInfo()
        {
            QuizesInfoQuestionsInfo = new HashSet<QuizesInfoQuestionsInfo>();
            QuizesInfoQuestionsInfoQuestionAnswerVariants = new HashSet<QuizesInfoQuestionsInfoQuestionAnswerVariants>();
        }

        [Key]
        [Column("QuizID")]
        public int QuizId { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        [StringLength(64)]
        public string Name { get; set; }
        public string Comment { get; set; }

        [InverseProperty("Quiz")]
        public virtual ICollection<QuizesInfoQuestionsInfo> QuizesInfoQuestionsInfo { get; set; }
        [InverseProperty("Quiz")]
        public virtual ICollection<QuizesInfoQuestionsInfoQuestionAnswerVariants> QuizesInfoQuestionsInfoQuestionAnswerVariants { get; set; }
    }
}
