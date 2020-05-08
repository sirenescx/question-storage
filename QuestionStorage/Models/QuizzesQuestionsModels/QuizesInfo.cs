using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.QuizzesQuestionsModels
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
        [Required(ErrorMessage = "Quiz date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Incorrect data format.")]
        public DateTime Date { get; set; }
        [StringLength(64)]
        [Required(ErrorMessage = "Quiz title is required.")]
        public string Name { get; set; }
        public string Comment { get; set; }

        [InverseProperty("Quiz")]
        public virtual ICollection<QuizesInfoQuestionsInfo> QuizesInfoQuestionsInfo { get; set; }
        [InverseProperty("Quiz")]
        public virtual ICollection<QuizesInfoQuestionsInfoQuestionAnswerVariants> QuizesInfoQuestionsInfoQuestionAnswerVariants { get; set; }
    }
}
