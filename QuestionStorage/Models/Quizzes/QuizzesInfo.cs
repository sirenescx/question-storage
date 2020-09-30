using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.Quizzes
{
    public class QuizzesInfo
    {
        public QuizzesInfo()
        {
            QuizzesInfoQuestionsInfo = new HashSet<QuizzesInfoQuestionsInfo>();
            QuizzesInfoQuestionsInfoQuestionAnswerVariants = new HashSet<QuizzesInfoQuestionsInfoQuestionAnswerVariants>();
        }

        public int QuizId { get; set; }
        [Column("Date")]
        [DataType(DataType.Date)]
        [MinLength(1)]
        [Required(ErrorMessage = "Question date is required.")]
        [Range(typeof(DateTime), "01/01/1900", "01/01/2100", ErrorMessage="Date is out of Range")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(1)]
        public string Name { get; set; }
        public string Comment { get; set; }
        public int CourseId { get; set; }

        public ICollection<QuizzesInfoQuestionsInfo> QuizzesInfoQuestionsInfo { get; set; }
        public ICollection<QuizzesInfoQuestionsInfoQuestionAnswerVariants> QuizzesInfoQuestionsInfoQuestionAnswerVariants { get; set; }
    }
}
