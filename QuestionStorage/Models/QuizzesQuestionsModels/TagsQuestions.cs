using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    [Table("Tags_Questions")]
    public partial class TagsQuestions
    {
        [Key]
        [Column("TagID")]
        public int TagId { get; set; }
        [Key]
        [Column("QuestID")]
        public int QuestId { get; set; }

        [ForeignKey(nameof(QuestId))]
        [InverseProperty(nameof(QuestionsInfo.TagsQuestions))]
        public virtual QuestionsInfo Quest { get; set; }
        [ForeignKey(nameof(TagId))]
        [InverseProperty(nameof(TagsInfo.TagsQuestions))]
        public virtual TagsInfo Tag { get; set; }
    }
}
