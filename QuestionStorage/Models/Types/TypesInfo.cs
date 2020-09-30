using System.Collections.Generic;
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

        public int TypeId { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        [InverseProperty("Type")]
        public virtual ICollection<QuestionsInfo> QuestionsInfo { get; set; }
    }
}
