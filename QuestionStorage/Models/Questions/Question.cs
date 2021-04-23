using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Quizzes;
using QuestionStorage.Models.Tags;
using QuestionStorage.Models.Types;

namespace QuestionStorage.Models.Questions
{
    public class Question
    {
        public Question()
        {
            AnswerOptions = new HashSet<AnswerOption>();
            QuizzesQuestions = new HashSet<QuizzesQuestions>();
            TagsQuestions = new HashSet<TagsQuestions>();
        }
        
        public int Id { get; set; }
        public int TypeId { get; set; }
        [Required(ErrorMessage = "Question title is required.")]
        [StringLength(256)]
        [MinLength(1)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Question content is required.")]
        [MinLength(1)]
        public string Text { get; set; }
        public string Xml { get; set; }
        public bool IsTemplate { get; set; }
        public int VersionId { get; set; }
        public int SourceId { get; set; }
        public int AuthorId { get; set; }
        public int CourseId { get; set; }
        public string Comment { get; set; }
        
        [ForeignKey(nameof(TypeId))]
        [InverseProperty(nameof(Types.Type.Questions))]
        public virtual Type Type { get; set; }
        
        [ForeignKey(nameof(CourseId))]
        [InverseProperty(nameof(Courses.Course.Questions))]
        public virtual Courses.Course Course { get; set; }
        
        [ForeignKey(nameof(AuthorId))]
        [InverseProperty(nameof(Users.User.Questions))]
        public virtual Users.User User { get; set; }
        
        [InverseProperty("Question")]
        public virtual ICollection<AnswerOption> AnswerOptions { get; set; }
        
        [InverseProperty("Question")]
        public virtual ICollection<QuizzesQuestions> QuizzesQuestions { get; set; }
        
        [InverseProperty("Question")]
        public virtual ICollection<TagsQuestions> TagsQuestions { get; set; }
    }
}
