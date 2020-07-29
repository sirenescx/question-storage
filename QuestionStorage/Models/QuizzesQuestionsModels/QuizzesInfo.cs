using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    public class QuizzesInfo
    {
        public QuizzesInfo()
        {
            QuizzesInfoQuestionsInfo = new HashSet<QuizzesInfoQuestionsInfo>();
            QuizzesInfoQuestionsInfoQuestionAnswerVariants = new HashSet<QuizzesInfoQuestionsInfoQuestionAnswerVariants>();
        }
        [Key]
        [Column("QuizID")]
        public int QuizId { get; set; }
        [Column("Date")]
        public DateTime Date { get; set; }
        [Column("Name")]
        [Required(ErrorMessage = "Question title is required.")]
        [StringLength(64)]
        public string Name { get; set; }
        public string Comment { get; set; }

        public ICollection<QuizzesInfoQuestionsInfo> QuizzesInfoQuestionsInfo { get; set; }
        public ICollection<QuizzesInfoQuestionsInfoQuestionAnswerVariants> QuizzesInfoQuestionsInfoQuestionAnswerVariants { get; set; }
        
    }
}