using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionStorage.Models.Tags
{
    public class Tag
    {
        public Tag()
        {
            InverseParent = new HashSet<Tag>();
            TagsQuestions = new HashSet<TagsQuestions>();
        }

        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public int CourseId { get; set; }
        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(InverseParent))]
        public virtual Tag Parent { get; set; }
        [InverseProperty(nameof(Parent))]
        public virtual ICollection<Tag> InverseParent { get; set; }
        [InverseProperty("Tag")]
        public virtual ICollection<TagsQuestions> TagsQuestions { get; set; }
    }
}
