using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    public class QuestionsInfo
    {
        public QuestionsInfo()
        {
            QuestionAnswerVariants = new HashSet<QuestionAnswerVariants>();
            TagsQuestions = new HashSet<TagsQuestions>();
        }

        [Key]
        [Column("QuestID")]
        public int QuestId { get; set; }
        [Column("TypeID")]
        public int TypeId { get; set; }
        [Required(ErrorMessage = "Question title is required.")]
        [StringLength(256)]
        [MinLengthAttribute(1)]
        public string QuestionName { get; set; }
        [Required(ErrorMessage = "Question content is required.")]
        [MinLengthAttribute(1)]
        public string QuestionText { get; set; }
        public string QuestionXml { get; set; }
        [ForeignKey(nameof(TypeId))]
        [InverseProperty(nameof(TypesInfo.QuestionsInfo))]
        public virtual TypesInfo Type { get; set; }
        public bool IsTemplate { get; set; }
        [InverseProperty("Quest")]
        public virtual ICollection<QuestionAnswerVariants> QuestionAnswerVariants { get; set; }
        [InverseProperty("Quest")]
        public virtual ICollection<TagsQuestions> TagsQuestions { get; set; }
    }
}
