using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.Quizzes
{
    public class Quiz
    {
        public Quiz()
        {
            QuizzesQuestions = new HashSet<QuizzesQuestions>();
            QuizzesAnswerOptions = new HashSet<QuizzesAnswerOptions>();
        }
        
        public int Id { get; set; }
        public DateTime Date { get; set; }
        
        public string Comment { get; set; }
        
        public int CourseId { get; set; }
        
        public string Name { get; set; }
        
        [ForeignKey(nameof(CourseId))]
        [InverseProperty(nameof(Courses.Course.Quizzes))]
        public virtual Courses.Course Course { get; set; }
        
        [InverseProperty("Quiz")]
        public virtual ICollection<QuizzesQuestions> QuizzesQuestions { get; set; }

        [InverseProperty("Quiz")]
        public virtual ICollection<QuizzesAnswerOptions> QuizzesAnswerOptions { get; set; }
        
    }
}