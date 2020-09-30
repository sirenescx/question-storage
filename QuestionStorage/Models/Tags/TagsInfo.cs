using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.Tags
{
    public class TagsInfo
    {
        public TagsInfo()
        {
            InverseParent = new HashSet<TagsInfo>();
            TagsQuestions = new HashSet<TagsQuestions>();
        }

        public int TagId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public int CourseId { get; set; }
        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(InverseParent))]
        public virtual TagsInfo Parent { get; set; }
        [InverseProperty(nameof(Parent))]
        public virtual ICollection<TagsInfo> InverseParent { get; set; }
        [InverseProperty("Tag")]
        public virtual ICollection<TagsQuestions> TagsQuestions { get; set; }
    }
}
