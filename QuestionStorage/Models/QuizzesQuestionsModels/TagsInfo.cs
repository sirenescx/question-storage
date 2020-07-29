using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.QuizzesQuestionsModels
{
    public class TagsInfo
    {
        public TagsInfo()
        {
            InverseParent = new HashSet<TagsInfo>();
            TagsQuestions = new HashSet<TagsQuestions>();
        }

        [Key]
        [Column("TagID")]
        public int TagId { get; set; }
        [Column("ParentID")]
        public int? ParentId { get; set; }
        [StringLength(64)]
        public string Name { get; set; }

        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(InverseParent))]
        public TagsInfo Parent { get; set; }
        [InverseProperty(nameof(Parent))]
        public ICollection<TagsInfo> InverseParent { get; set; }
        [InverseProperty("Tag")]
        public ICollection<TagsQuestions> TagsQuestions { get; set; }
    }
}
