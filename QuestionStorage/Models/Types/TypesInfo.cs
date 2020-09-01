using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuestionStorage.Models.Questions;

namespace QuestionStorage.Models.Types
{
    public class TypesInfo
    {
        public TypesInfo()
        {
            QuestionsInfo = new HashSet<QuestionsInfo>();
        }

        [Key]
        [Column("TypeID")]
        public int TypeId { get; set; }
        [StringLength(64)]
        public string Name { get; set; }
        [StringLength(256)]
        public string Comment { get; set; }

        [InverseProperty("Type")]
        public virtual ICollection<QuestionsInfo> QuestionsInfo { get; set; }
    }
}
