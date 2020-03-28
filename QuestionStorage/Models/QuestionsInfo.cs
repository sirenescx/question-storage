using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models
{
    public partial class QuestionsInfo
    {
        public QuestionsInfo()
        {
            QuestionAnswerVariants = new HashSet<QuestionAnswerVariants>();
            QuizesInfoQuestionsInfo = new HashSet<QuizesInfoQuestionsInfo>();
            TagsQuestions = new HashSet<TagsQuestions>();
        }

        [Key]
        [Column("QuestID")]
        public int QuestId { get; set; }
        [Column("RubricID")]
        public int? RubricId { get; set; }
        [Column("TypeID")]
        public int TypeId { get; set; }
        public int Flags { get; set; }
        [StringLength(256)]
        public string QuestionName { get; set; }
        public string QuestionText { get; set; }

        [ForeignKey(nameof(RubricId))]
        [InverseProperty(nameof(RubricsInfo.QuestionsInfo))]
        public virtual RubricsInfo Rubric { get; set; }
        [ForeignKey(nameof(TypeId))]
        [InverseProperty(nameof(TypesInfo.QuestionsInfo))]
        public virtual TypesInfo Type { get; set; }
        [InverseProperty("Quest")]
        public virtual ICollection<QuestionAnswerVariants> QuestionAnswerVariants { get; set; }
        [InverseProperty("Quest")]
        public virtual ICollection<QuizesInfoQuestionsInfo> QuizesInfoQuestionsInfo { get; set; }
        [InverseProperty("Quest")]
        public virtual ICollection<TagsQuestions> TagsQuestions { get; set; }
    }
}
