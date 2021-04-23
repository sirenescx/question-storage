using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Quizzes;

namespace QuestionStorage.Models.Questions
{
    public class AnswerOption
    {
        public AnswerOption()
        {
            QuizzesAnswerOptions = new HashSet<QuizzesAnswerOptions>();
        }

        public int Id { get; set; }
        public int QuestionId { get; set; }

        [StringLength(256)]
        [Required(ErrorMessage = "Response option text is required.")]
        [MinLength(1)]
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        [ForeignKey(nameof(QuestionId))]
        [InverseProperty(nameof(Questions.Question.AnswerOptions))]
        public virtual Question Question { get; set; }
        public virtual ICollection<QuizzesAnswerOptions> QuizzesAnswerOptions { get; set; }
    }
}
