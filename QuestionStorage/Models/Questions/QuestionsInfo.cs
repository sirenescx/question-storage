using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Quizzes;
using QuestionStorage.Models.Tags;
using QuestionStorage.Models.Types;

namespace QuestionStorage.Models.Questions
{
    public class QuestionsInfo
    {
        public QuestionsInfo()
        {
            QuestionAnswerVariants = new HashSet<QuestionAnswerVariants>();
            QuizzesInfoQuestionsInfo = new HashSet<QuizzesInfoQuestionsInfo>();
            TagsQuestions = new HashSet<TagsQuestions>();
        }

        public int QuestId { get; set; }
        public int TypeId { get; set; }
        [Required(ErrorMessage = "Question title is required.")]
        [StringLength(256)]
        [MinLength(1)]
        public string QuestionName { get; set; }
        [Required(ErrorMessage = "Question content is required.")]
        [MinLength(1)]
        public string QuestionText { get; set; }
        public string QuestionXml { get; set; }
        public bool IsTemplate { get; set; }
        public int VersionId { get; set; }
        public int SourceQuestId { get; set; }
        public int AuthorId { get; set; }
        public int CourseId { get; set; }
        [ForeignKey(nameof(TypeId))]
        [InverseProperty(nameof(TypesInfo.QuestionsInfo))]
        public virtual TypesInfo Type { get; set; }
        [InverseProperty("Quest")]
        public virtual ICollection<QuestionAnswerVariants> QuestionAnswerVariants { get; set; }
        [InverseProperty("Quest")]
        public virtual ICollection<QuizzesInfoQuestionsInfo> QuizzesInfoQuestionsInfo { get; set; }
        [InverseProperty("Quest")]
        public virtual ICollection<TagsQuestions> TagsQuestions { get; set; }
    }
}
